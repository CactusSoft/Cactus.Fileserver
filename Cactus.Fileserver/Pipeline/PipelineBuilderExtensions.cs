using System;
using System.Linq;
using System.Net.Http;

namespace Cactus.Fileserver.Pipeline
{
    public static class PipelineBuilderExtensions
    {
        public static PipelineBuilder UseMultipartContent(
            this PipelineBuilder builder)
        {
            return builder.Use(next => async (request, content, stream, info) =>
            {
                //Extract multipart if need
                if (content.IsMimeMultipartContent())
                {
                    var provider = await content.ReadAsMultipartAsync();
                    var firstFileContent =
                        provider.Contents.FirstOrDefault(
                            c => !string.IsNullOrWhiteSpace(c.Headers.ContentDisposition.FileName));
                    if (firstFileContent != null)
                        return await next(request, firstFileContent, stream, info);
                    throw new ArgumentException("Multipart content detected, but no files found inside.");
                }
                return await next(request, content, stream, info);
            });
        }

        public static PipelineBuilder ExtractFileinfo(
            this PipelineBuilder builder)
        {
            return builder.Use(next => async (request, content, stream, info) =>
            {
                //Set file info
                info.MimeType = content.Headers.ContentType.ToString();
                info.OriginalName = content.Headers.ContentDisposition.FileName?.Trim('"') ?? "noname";
                return await next(request, content, stream, info);
            });
        }

        public static PipelineBuilder ReadContentStream(
            this PipelineBuilder builder)
        {
            return builder.Use(next => async (request, content, stream, info) =>
                await next(request, content, await content.ReadAsStreamAsync(), info));
        }

        public static FileProcessorDelegate Store(
            this PipelineBuilder builder, IFileStorageService fileStorageService)
        {
            return builder.Run(async (request, content, stream, info) =>
            {
                if (stream != null)
                    return await fileStorageService.Create(stream, info);
                if (content != null)
                    return await fileStorageService.Create(await content.ReadAsStreamAsync(), info);
                if (request?.Body != null)
                    return await fileStorageService.Create(request.Body, info);
                throw new ArgumentException("Nothing to store: no stream, neither content, neither request.Body");
            });

        }
    }
}