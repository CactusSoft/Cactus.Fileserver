using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;
using Cactus.Fileserver.LocalStorage;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileserverBaseConfig<TRequest, TMeta> : IFileserverConfig<TRequest, TMeta> where TMeta : IFileInfo
    {
        private readonly Uri _baseFileserverUri;
        private readonly string _bucketName;
        private readonly string _awsSecretKey;
        private readonly string _awsAccessKey;
        private readonly string _metaStorageFolder;


        private readonly RegionEndpoint _regionEndpoint;

        public S3FileserverBaseConfig(Uri baseFileserverUri, string bucketName, string awsSecretKey, string awsAccessKey, RegionEndpoint regionEndpoint, string metaStorageFolder)
        {
            _baseFileserverUri = baseFileserverUri;
            _bucketName = bucketName;
            _awsSecretKey = awsSecretKey;
            _awsAccessKey = awsAccessKey;
            _regionEndpoint = regionEndpoint;
            _metaStorageFolder = metaStorageFolder;
        }

        public string Path { get; }

        public Func<IFileStorageService<TMeta>> FileStorage
        {
            get
            {
                return () =>
                {
                    var fileStorage = new S3FileStorage<TMeta>(_bucketName,_regionEndpoint,_awsAccessKey,_awsSecretKey,_baseFileserverUri,
                        new RandomNameProvider<TMeta> { StoreExt = true });
                    var metaStorage = new LocalMetaInfoStorage<TMeta>(_metaStorageFolder);
                    return new FileStorageService<TMeta>(metaStorage, fileStorage, new DummySecurityManager());
                };
            }
        }
        public Func<Func<TRequest, HttpContent, TMeta, Task<TMeta>>> NewFilePipeline { get; protected set; }
        public Func<Func<TRequest, IFileGetContext<TMeta>, Task>> GetFilePipeline { get; protected set; }

        protected class DummySecurityManager : ISecurityManager
        {
            public bool MayCreate(IFileInfo file)
            {
                return true;
            }

            //No delete
            public bool MayDelete(IFileInfo file)
            {
                return true;
            }

            public bool MayRead(IFileInfo info)
            {
                return true;
            }
        }
    }
}