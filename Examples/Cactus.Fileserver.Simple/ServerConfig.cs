using System;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Storage;
using Cactus.Fileserver.ImageResizer.Core;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Cactus.Fileserver.LocalStorage;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Simple
{
    public class ServerConfig : LocalFileserverConfig
    {
        private readonly Instructions _defaultImageInstructions = new Instructions("");
        private readonly Instructions _mandatoryImageInstructions = new Instructions("maxwidth=1440&maxheight=1440");

        private ImageResizerService ImgResizerResolver() => new ImageResizerService(_defaultImageInstructions, _mandatoryImageInstructions);

        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            NewFilePipeline = BuildPipeline;
        }

        private FileProcessorDelegate BuildPipeline()
        {
            return new PipelineBuilder()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .RunStoreFileAsIs(FileStorage);
        }

        public override IApplicationBuilder GetPipeline(IApplicationBuilder app)
        {
            return base.GetPipeline(app.UseMiddleware<DynamicResizeMiddleware>(ImgResizerResolver(), FileStorage()));
        }
    }
}
