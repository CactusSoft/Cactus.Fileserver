using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.S3Storage;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Simple
{
    public class ServerConfig : LocalFileserverConfig<HttpRequest, ExtendedMetaInfo>
    {
        private readonly Instructions _defaultImageInstructions = new Instructions("");
        private readonly Instructions _mandatoryImageInstructions = new Instructions("maxwidth=1440&maxheight=1440");

        private ImageResizerService ImgResizerResolver() => new ImageResizerService(_defaultImageInstructions, _mandatoryImageInstructions);



        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder,metaStorageFolder,baseUri,null)
        {
            NewFilePipeline = BuildPipeline;
            GetFilePipeline = BuildGetPipeline;
        }

        private Func<HttpRequest, HttpContent, ExtendedMetaInfo, Task<ExtendedMetaInfo>> BuildPipeline()
        {


            return new PipelineBuilder<ExtendedMetaInfo>()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .RunStoreFileAsIs(FileStorage);
        }

        private Func<HttpRequest, IFileGetContext<ExtendedMetaInfo>, Task> BuildGetPipeline()
        {

            return new PipelineBuilder<ExtendedMetaInfo>()
                .EnableDynamicResizing(FileStorage, ImgResizerResolver)
                .GetStoreFileAsIs(FileStorage);
        }
    }
}
