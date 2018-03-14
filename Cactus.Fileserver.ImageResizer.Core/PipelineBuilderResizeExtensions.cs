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
    public static class PipelineBuilderResizeExtensions
    {

        public static GenericPipelineBuilder<HttpRequest, TMeta> EnableDynamicResizing<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder, Func<IFileStorageService<TMeta>> storageServceResolverFunc, Func<ImageResizerService> imageResizerResolver) where TMeta : class, IExtendedFileInfo, new()
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
                            Extra = metaData.Extra?.ToDictionary(k=>k.Key,v=>v.Value),
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