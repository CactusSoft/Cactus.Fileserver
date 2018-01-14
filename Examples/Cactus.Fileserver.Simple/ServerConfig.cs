using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.LocalStorage;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Simple
{
    public class ServerConfig : LocalFileserverConfig<HttpRequest>
    {
        public ServerConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri)
            : base(fileStorageFolder, metaStorageFolder, baseUri, null)
        {
            var newFilePipeline = BuildPipeline();
            NewFilePipeline = () => newFilePipeline;
        }

        private Func<HttpRequest, HttpContent, IFileInfo, Task<MetaInfo>> BuildPipeline()
        {
            return new PipelineBuilder()
                .UseMultipartRequestParser()
                .UseOriginalFileinfo()
                .RunStoreFileAsIs(FileStorage);
        }
    }
}
