using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.AspNetCore
{
    public class PipelineBuilder<TMeta> : GenericPipelineBuilder<HttpRequest,TMeta> where TMeta : IFileInfo
    {
    }

    public static class PipelineBuilderExtensions
    {
        public static GenericPipelineBuilder<HttpRequest, TMeta> UseMultipartRequestParser<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder) where TMeta : IFileInfo
        {
            return builder.Use(next => async (request, content, info) =>
            {
                //Extract multipart if need
                if (content.IsMimeMultipartContent())
                {
                    var provider = await content.ReadAsMultipartAsync();
                    var firstFileContent =
                        provider.Contents.FirstOrDefault(
                            c => !string.IsNullOrWhiteSpace(c.Headers.ContentDisposition.FileName));
                    if (firstFileContent != null)
                        return await next(request, firstFileContent, info);
                    throw new ArgumentException("Multipart content detected, but no files found inside.");
                }
                return await next(request, content, info);
            });
        }

        public static GenericPipelineBuilder<HttpRequest, TMeta> UseOriginalFileinfo<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder) where TMeta : IFileInfo
        {
            return builder.Use(next => async (request, content, info) =>
            {
                //Set file info
                info.MimeType = content.Headers.ContentType.ToString();
                info.OriginalName = content.Headers.ContentDisposition.FileName?.Trim('"') ?? "noname";
                return await next(request, content, info);
            });
        }

        public static Func<HttpRequest, HttpContent, TMeta, Task<TMeta>> RunStoreFileAsIs<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder, Func<IFileStorageService<TMeta>> storageServceResolverFunc) where TMeta : IFileInfo 
        {
            return builder.Run(async (request, content, info) =>
            {
                if (info is IExtendedFileInfo extendedMeta)
                {
                    extendedMeta.IsOriginal = true;
                }
                var fileStorage = storageServceResolverFunc();
                using (var stream = await content.ReadAsStreamAsync())
                {
                    return await fileStorage.Create(stream, info);
                }
            });
        }

        public static Func<HttpRequest, IFileGetContext<TMeta>, Task> GetStoreFileAsIs<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder, Func<IFileStorageService<TMeta>> storageServceResolverFunc) where TMeta : IFileInfo
        {
            return builder.Run(async (request, getContext) =>
            {
                var storage = storageServceResolverFunc();
                try
                {
                    getContext.RedirectUri = storage.GetRedirectUri(request.GetAbsoluteUri());
                    getContext.IsNeedToRedirect = true;
                    getContext.IsNeedToPromoteStream = false;
                }
                catch (NotImplementedException)
                {
                    var stream = await storage.Get(request.GetAbsoluteUri());
                    if (getContext.ContextStream.CanWrite)
                    {
                        await stream.CopyToAsync(getContext.ContextStream);
                        getContext.IsNeedToPromoteStream = true;
                        getContext.IsNeedToRedirect = false;
                    }
                }
            });
        }


        public static Func<HttpRequest, IFileGetContext<TMeta>, Task> DisableGet<TMeta>(
            this GenericPipelineBuilder<HttpRequest, TMeta> builder) where TMeta : IFileInfo
        {
            return builder.Run((request, outStream) => Task.CompletedTask);
        }
    }
}