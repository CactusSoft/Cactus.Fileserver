using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.AspNetCore
{
    public delegate Task<MetaInfo> FileProcessorDelegate(HttpRequest request, HttpContent content, IFileInfo info);

    public delegate FileProcessorDelegate PipelineDelegate(FileProcessorDelegate next);

    public class PipelineBuilder
    {
        private readonly IList<PipelineDelegate> _processors = new List<PipelineDelegate>();

        public PipelineBuilder Use(PipelineDelegate processor)
        {
            _processors.Add(processor);
            return this;
        }

        //Run for "add" pipeline
        public FileProcessorDelegate Run(FileProcessorDelegate finalizer)
        {
            if (_processors.Count == 0)
                return finalizer;

            return _processors.Reverse().Aggregate(finalizer, (current, processor) => processor(current));
        }
    }


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
            this PipelineBuilder builder, Func<IFileStorageService> storageServceResolver)
        {
            return builder.Run(async (request, content, info) =>
            {
                var fileStorage = storageServceResolver();
                using (var stream = await content.ReadAsStreamAsync())
                {
                    return await fileStorage.Create(stream, info);
                }
            });
        }
    }
}