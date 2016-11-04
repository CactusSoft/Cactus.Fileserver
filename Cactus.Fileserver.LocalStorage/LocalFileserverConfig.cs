using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileserverConfig<T> : IFileserverConfig<T>
    {
        private readonly string fileStorageFolder;
        private readonly string metaStorageFolder;

        public LocalFileserverConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri, string path)
        {
            this.fileStorageFolder = fileStorageFolder;
            this.metaStorageFolder = metaStorageFolder;
            BaseUri = baseUri;
            Path = path;
            this.fileStorageFolder = fileStorageFolder;
            SecurityManager = () => new NothingCheckSecurityManager();
        }

        public string Path { get; }

        public Uri BaseUri { get; }

        public Func<ISecurityManager> SecurityManager { get; set; }

        public Func<IFileStorageService> FileStorage
        {
            get
            {
                return () =>
                {
                    var baseFilesUri = (string.IsNullOrEmpty(Path) || Path == "/")
                        ? BaseUri
                        : new Uri(BaseUri, Path + '/');
                    var fileStorage = new LocalFileStorage<MetaInfo>(baseFilesUri, new RandomNameProvider<MetaInfo> { StoreExt = true }, fileStorageFolder);
                    var metaStorage = new LocalMetaInfoStorage<MetaInfo>(metaStorageFolder);
                    return new FileStorageService<MetaInfo>(metaStorage, fileStorage, SecurityManager());
                };
            }
        }

        public Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>> NewFilePipeline { get; set; }
    }
}
