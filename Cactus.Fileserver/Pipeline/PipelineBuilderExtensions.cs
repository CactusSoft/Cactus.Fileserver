using System;
using System.Linq;
using System.Net.Http;

namespace Cactus.Fileserver
{
    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder UseMultipartRequestParser(
            this PipelineBuilder builder)
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

        public static PipelineBuilder UseOriginalFileinfo(
            this PipelineBuilder builder)
        {
            return builder.Use(next => async (request, content, info) =>
            {
                //Set file info
                info.MimeType = content.Headers.ContentType.ToString();
                info.OriginalName = content.Headers.ContentDisposition.FileName?.Trim('"') ?? "noname";
                return await next(request, content, info);
            });
        }

        public static FileProcessorDelegate RunStoreFileAsIs(
            this PipelineBuilder builder, IFileStorageService fileStorageService)
        {
            return builder.Run(async (request, content, info) =>
            {
                
                using (var stream = await content.ReadAsStreamAsync())
                {
                    return await fileStorageService.Create(stream, info);
                }
            });
        }
    }
}