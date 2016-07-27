using System;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.Asp5.Config
{
    public class LocalFileserverConfig : IFileserverConfig
    {
        private readonly string storageFolder;

        public LocalFileserverConfig(string storageFolder, Uri baseUri, string path)
        {
            this.storageFolder = storageFolder;
            BaseUri = baseUri;
            Path = path;
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
                    var fileStorage = new LocalFileStorage<MetaInfo>(storageFolder, new Uri(BaseUri, Path + '/'), new RandomNameProvider<MetaInfo>());
                    var metaStorage = new LocalMetaInfoStorage<MetaInfo>(storageFolder);
                    return new FileStorageService<MetaInfo>(metaStorage, fileStorage, SecurityManager());
                };
            }
        }
    }
}
