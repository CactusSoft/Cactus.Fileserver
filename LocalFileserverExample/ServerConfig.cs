using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.LocalStorage;
using ImageResizer;
using Microsoft.AspNetCore.Http;

namespace LocalFileserver
{
    public class ServerConfig : LocalFileserverConfig<HttpRequest>
    {
        private readonly Func<HttpRequest, HttpContent, IFileInfo, Task<MetaInfo>> newFilePipeline;

        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri) : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            newFilePipeline = BuildPipeline();
            NewFilePipeline = () => newFilePipeline;
        }

        private Func<HttpRequest, HttpContent, IFileInfo, Task<MetaInfo>> BuildPipeline()
        {
            var defaultImageInstructions = new Instructions("autorotate=true");
            var mandatoryImageInstructions = new Instructions("maxwidth=300&maxheight=400");
            var defaultThumbnailInstructions = new Instructions("width=100&height=100");
            var mandatoryThumbnailInstructions = new Instructions("maxwidth=300&maxheight=400");
            Func<ImageStorageService> imgStorageResolver = () =>
            new ImageStorageService(
                FileStorage(),
                defaultImageInstructions,
                mandatoryImageInstructions,
                defaultThumbnailInstructions,
                mandatoryThumbnailInstructions
                );

            return new PipelineBuilder()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .Use(next => (async (request, content, info) =>
                {
                    //Process image + thumbnail if requested or call next otherwise
                    if (content.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    {
                        var imgStorage = imgStorageResolver();
                        if (request.QueryString.HasValue && request.Query.ContainsKey("thumbnail"))
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
