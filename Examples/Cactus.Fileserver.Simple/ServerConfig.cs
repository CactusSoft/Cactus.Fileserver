using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Cactus.Fileserver.LocalStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Simple
{
    public class ServerConfig : LocalFileserverConfig<ExtendedMetaInfo>
    {
        private readonly Instructions _defaultImageInstructions = new Instructions("");
        private readonly Instructions _mandatoryImageInstructions = new Instructions("maxwidth=1440&maxheight=1440");

        private ImageResizerService ImgResizerResolver() => new ImageResizerService(_defaultImageInstructions, _mandatoryImageInstructions);

        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            NewFilePipeline = BuildPipeline;
        }

        private Func<HttpRequest, HttpContent, ExtendedMetaInfo, Task<ExtendedMetaInfo>> BuildPipeline()
        {
            return new PipelineBuilder<ExtendedMetaInfo>()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .RunStoreFileAsIs(FileStorage);
        }

        public override IApplicationBuilder GetPipeline(IApplicationBuilder app)
        {
            var resizer = new ImageResizerService(_defaultImageInstructions, _mandatoryImageInstructions);
            return base.GetPipeline(app.UseMiddleware<DynamicResizeMiddleware<ExtendedMetaInfo>>(ImgResizerResolver(), FileStorage()));
        }
    }
}
