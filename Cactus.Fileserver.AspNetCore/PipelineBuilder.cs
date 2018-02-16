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
    public class PipelineBuilder : GenericPipelineBuilder<HttpRequest>
    {
    }

    public static class PipelineBuilderExtensions
    {
        public static GenericPipelineBuilder<HttpRequest> UseMultipartRequestParser(
            this GenericPipelineBuilder<HttpRequest> builder)
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

        public static GenericPipelineBuilder<HttpRequest> UseOriginalFileinfo(
            this GenericPipelineBuilder<HttpRequest> builder)
        {
            return builder.Use(next => async (request, content, info) =>
            {
                //Set file info
                info.MimeType = content.Headers.ContentType.ToString();
                info.OriginalName = content.Headers.ContentDisposition.FileName?.Trim('"') ?? "noname";
                return await next(request, content, info);
            });
        }

        public static Func<HttpRequest, HttpContent, IFileInfo, Task<MetaInfo>> RunStoreFileAsIs(
            this GenericPipelineBuilder<HttpRequest> builder, Func<IFileStorageService> storageServceResolverFunc)
        {
            return builder.Run(async (request, content, info) =>
            {
                var fileStorage = storageServceResolverFunc();
                using (var stream = await content.ReadAsStreamAsync())
                {
                    return await fileStorage.Create(stream, info);
                }
            });
        }


        public static Func<HttpRequest, Stream, Task> GetStoreFileAsIs(
            this GenericPipelineBuilder<HttpRequest> builder, Func<IFileStorageService> storageServceResolverFunc)
        {
            return builder.Run( async (request, outStream) =>
            {
                var fileStorage = storageServceResolverFunc();
                var result = await fileStorage.Get(request.GetAbsoluteUri());
                await result.CopyToAsync(outStream); 
            });
        }

        public static Func<HttpRequest, Stream, Task> DisableGet(
            this GenericPipelineBuilder<HttpRequest> builder)
        {
            return builder.Run((request, outStream) =>
            {
                outStream=Stream.Null;
                return Task.CompletedTask;
            });
        }
    }
}