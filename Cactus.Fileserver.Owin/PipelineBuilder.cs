using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;

namespace Cactus.Fileserver.Owin
{
    public class PipelineBuilder<TMeta> : GenericPipelineBuilder<IOwinRequest, TMeta> where TMeta : IFileInfo
    {
        public PipelineBuilder<TMeta> UseMultipartRequestParser()
        {
            Use(next => async (request, content, info) =>
            {
                //Extract multipart if need
                if (content.IsMimeMultipartContent())
                {
                    var provider = await content.ReadAsMultipartAsync();
                    var firstFileContent =
                        provider.Contents.FirstOrDefault(
                            c => !string.IsNullOrWhiteSpace(c.Headers.ContentDisposition.FileName));
                    if (firstFileContent != null)
                    {
                        return await next(request, firstFileContent, info);
                    }
                    throw new ArgumentException("Multipart content detected, but no files found inside.");
                }
                return await next(request, content, info);
            });
            return this;
        }

        public PipelineBuilder<TMeta> UseOriginalFileinfo()
        {
            Use(next => async (request, content, info) =>
            {
                //Set file info
                info.MimeType = content.Headers.ContentType.ToString();
                info.OriginalName = content.Headers.ContentDisposition.FileName?.Trim('"') ?? "noname";
                return await next(request, content, info);
            });
            return this;
        }

        public Func<IOwinRequest, HttpContent, TMeta, Task<TMeta>> RunStoreFileAsIs(Func<IFileStorageService<TMeta>> storageServceResolverFunc)
        {
            return Run(async (request, content, info) =>
            {
                var fileStorage = storageServceResolverFunc();
                using (var stream = await content.ReadAsStreamAsync())
                {
                    return await fileStorage.Create(stream, info);
                }
            });
        }

        public new PipelineBuilder<TMeta> Use(Func<Func<IOwinRequest, HttpContent, TMeta, Task<TMeta>>, Func<IOwinRequest, HttpContent, TMeta, Task<TMeta>>> processor)
        {
            base.Use(processor);
            return this;
        }
    }

    public static class PipelineBuilderExtensions
    {
        
    }
}
