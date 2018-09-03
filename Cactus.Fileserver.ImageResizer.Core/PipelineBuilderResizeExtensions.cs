using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.ImageResizer.Core
{
    public class DynamicResizeMiddleware<TMeta> where TMeta : class, IExtendedFileInfo, new()
    {
        private readonly IFileStorageService<TMeta> _storage;
        private readonly ImageResizerService _resizer;
        private readonly RequestDelegate _next;

        public DynamicResizeMiddleware(RequestDelegate next, ImageResizerService resizer, IFileStorageService<TMeta> storage)
        {
            _next = next;
            _resizer = resizer;
            _storage = storage;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requset = context.Request;
            var metaData = _storage.GetInfo(requset.GetAbsoluteUri());
            if (requset.QueryString.HasValue && metaData.MimeType.StartsWith("image") &&
                !metaData.MimeType.EndsWith("gif"))
            {
                var instructions = new Instructions(requset.QueryString.Value);
                var sizeKey = instructions.GetSizeKey();
                if (!metaData.IsOriginal || sizeKey == null)
                {
                    await _next(context);
                }
                if (metaData.AvaliableSizes.TryGetValue(sizeKey, out var redirectUri))
                {
                    context.Response.Redirect(redirectUri.ToString(), true);
                    return;
                }

                using (var tempFile = new MemoryStream())
                using (var original = await _storage.Get(requset.GetAbsoluteUri()))
                {
                    _resizer.ProcessImage(original, tempFile, instructions);
                    var newFileInfo = new TMeta
                    {
                        IsOriginal = false,
                        OriginalUri = metaData.OriginalUri,
                        Icon = metaData.Icon,
                        MimeType = metaData.MimeType,
                        OriginalName = metaData.OriginalName,
                        Extra = metaData.Extra?.ToDictionary(k => k.Key, v => v.Value),
                        Owner = metaData.Owner,
                        StoragePath = metaData.StoragePath
                    };
                    tempFile.Position = 0;
                    var result = await _storage.Create(tempFile, newFileInfo);
                    Uri savedRedirectUri = null;
                    try
                    {
                        savedRedirectUri = _storage.GetRedirectUri(result.Uri);
                    }
                    catch (NotImplementedException)
                    {
                        savedRedirectUri = result.Uri;
                    }
                    finally
                    {
                        metaData.AvaliableSizes.Add(sizeKey, savedRedirectUri);
                        await _storage.UpdateMetadata(metaData);
                        context.Response.Redirect(savedRedirectUri.ToString(), true);
                    }
                    return;
                }
            }
            await _next(context);
        }
    }

    public static class PipelineBuilderResizeExtensions
    {
        public static GenericPipelineBuilder<TMeta> EnableDynamicResizing<TMeta>(
            this GenericPipelineBuilder<TMeta> builder, Func<IFileStorageService<TMeta>> storageServceResolverFunc, Func<ImageResizerService> imageResizerResolver) where TMeta : class, IExtendedFileInfo, new()
        {
            return builder.Use(next => async (requset, getContext) =>
            {
                var fileStore = storageServceResolverFunc();
                var metaData = fileStore.GetInfo(requset.GetAbsoluteUri());
                if (requset.QueryString.HasValue && metaData.MimeType.StartsWith("image") &&
                    !metaData.MimeType.EndsWith("gif"))
                {
                    var instructions = new Instructions(requset.QueryString.Value);
                    var sizeKey = instructions.GetSizeKey();
                    if (!metaData.IsOriginal || sizeKey == null)
                    {
                        await next(requset, getContext);
                    }
                    if (metaData.AvaliableSizes.TryGetValue(sizeKey, out var redirectUri))
                    {
                        getContext.RedirectUri = redirectUri;
                        getContext.IsNeedToRedirect = true;
                        return;
                    }

                    var imgResizer = imageResizerResolver();
                    using (var tempFile = new MemoryStream())
                    using (var original = await fileStore.Get(requset.GetAbsoluteUri()))
                    {
                        imgResizer.ProcessImage(original, tempFile, instructions);
                        var newFileInfo = new TMeta
                        {
                            IsOriginal = false,
                            OriginalUri = metaData.OriginalUri,
                            Icon = metaData.Icon,
                            MimeType = metaData.MimeType,
                            OriginalName = metaData.OriginalName,
                            Extra = metaData.Extra?.ToDictionary(k => k.Key, v => v.Value),
                            Owner = metaData.Owner,
                            StoragePath = metaData.StoragePath
                        };
                        tempFile.Position = 0;
                        var result = await fileStore.Create(tempFile, newFileInfo);
                        Uri savedRedirectUri = null;
                        try
                        {
                            savedRedirectUri = fileStore.GetRedirectUri(result.Uri);
                        }
                        catch (NotImplementedException)
                        {
                            savedRedirectUri = result.Uri;
                        }
                        finally
                        {
                            metaData.AvaliableSizes.Add(sizeKey, savedRedirectUri);
                            await fileStore.UpdateMetadata(metaData);
                            getContext.RedirectUri = savedRedirectUri;
                            getContext.IsNeedToRedirect = true;
                        }
                        return;
                    }
                }
                await next(requset, getContext);
            });
        }
    }
}