using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Cactus.Fileserver.LocalStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32.SafeHandles;

namespace Cactus.Fileserver.Simple
{
    public class ServerConfig : LocalFileserverConfig<HttpRequest>
    {
        private readonly Instructions _defaultImageInstructions = new Instructions("");
        private readonly Instructions _mandatoryImageInstructions = new Instructions("maxwidth=300&maxheight=400");
        private readonly Instructions _defaultThumbnailInstructions = new Instructions("width=100&height=100");
        private readonly Instructions _mandatoryThumbnailInstructions = new Instructions("maxwidth=300&maxheight=400");

        private ImageStorageService ImgStorageResolver() => new ImageStorageService(FileStorage(), _defaultImageInstructions, _mandatoryImageInstructions, _defaultThumbnailInstructions, _mandatoryThumbnailInstructions);


        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            NewFilePipeline = BuildPipeline;
            GetFilePipeline = BuildGetPipeline;
        }

        private Func<HttpRequest, HttpContent, IFileInfo, Task<MetaInfo>> BuildPipeline()
        {


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

        private Func<HttpRequest, Stream, Task> BuildGetPipeline()
        {

            return new PipelineBuilder()
                .Use(next => async (requset, stream) =>
                {
                    if(requset.QueryString.HasValue)
                    {
                        var imgStorage = ImgStorageResolver();
                        var fileStore = FileStorage();
                        var metaData = fileStore.GetInfo(requset.GetAbsoluteUri());
                        if (metaData.MimeType.StartsWith("image") && !metaData.MimeType.EndsWith("gif"))
                        {
                            var instructions = new Instructions(requset.QueryString.Value);
                            instructions.Join(_mandatoryImageInstructions);
                            using (var original = await fileStore.Get(requset.GetAbsoluteUri()))
                            {
                                imgStorage.ProcessImage(original, stream, instructions);
                                original.Close();
                            }
                        }
                    }
                    await next(requset,stream);
                })
                .GetStoreFileAsIs(FileStorage);
        }
    }
}
