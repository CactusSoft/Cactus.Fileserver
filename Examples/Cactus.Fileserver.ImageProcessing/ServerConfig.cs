using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.Owin;
using ImageResizer;
using Microsoft.Owin;

namespace Cactus.Fileserver.ImageProcessing
{
    public class ServerConfig : LocalFileserverConfig<IOwinRequest>
    {
        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            var newFilePipeline = BuildPipeline();
            NewFilePipeline = () => newFilePipeline;
        }

        private Func<IOwinRequest, HttpContent, IFileInfo, Task<MetaInfo>> BuildPipeline()
        {
            var defaultImageInstructions = new Instructions("autorotate=true");
            var mandatoryImageInstructions = new Instructions("maxwidth=300&maxheight=400");
            var defaultThumbnailInstructions = new Instructions("width=100&height=100");
            var mandatoryThumbnailInstructions = new Instructions("maxwidth=300&maxheight=400");
            ImageStorageService ImgStorageResolver() => new ImageStorageService(FileStorage(), defaultImageInstructions, mandatoryImageInstructions, defaultThumbnailInstructions, mandatoryThumbnailInstructions);

            return new PipelineBuilder()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .Use(next => (async (request, content, info) =>
                {
                    //Process image + thumbnail if requested or call next otherwise
                    if (content.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        var imgStorage = ImgStorageResolver();
                        if (request.QueryString.HasValue && request.Query.Any(x => x.Key.Equals("thumbnail", StringComparison.OrdinalIgnoreCase)))
                        {
                            var bytes = await content.ReadAsByteArrayAsync();
                            return await imgStorage.StoreWithThumbnail(info, bytes, request.QueryString.ToString());
                        }
                        using (var stream = await content.ReadAsStreamAsync())
                        {
                            return await imgStorage.StoreSingle(info, stream, request.QueryString.ToString());
                        }
                    }
                    return await next(request, content, info);
                }))
                .RunStoreFileAsIs(FileStorage);
        }
    }
}
