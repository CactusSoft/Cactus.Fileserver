using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.AspNetCore
{
    public class PipelineBuilder<TMeta> : GenericPipelineBuilder<TMeta> where TMeta : IFileInfo
    {
    }

    public static class PipelineBuilderExtensions
    {
        public static GenericPipelineBuilder<TMeta> UseMultipartRequestParser<TMeta>(
            this GenericPipelineBuilder<TMeta> builder) where TMeta : IFileInfo
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

        public static GenericPipelineBuilder<TMeta> UseOriginalFileinfo<TMeta>(
            this GenericPipelineBuilder<TMeta> builder) where TMeta : IFileInfo
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
            this GenericPipelineBuilder<TMeta> builder, Func<IFileStorageService<TMeta>> storageServceResolverFunc) where TMeta : IFileInfo 
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
        
    }
}