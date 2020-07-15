using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Security;
using Cactus.Fileserver.Storage;

namespace Cactus.Fileserver
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IMetaInfoStorage _metaStorage;
        private readonly ISecurityManager _securityManager;

        public FileStorageService(IMetaInfoStorage metaStorage, IFileStorage fileStorage,
            ISecurityManager securityManager)
        {
            _metaStorage = metaStorage;
            _fileStorage = fileStorage;
            _securityManager = securityManager;
        }

        public async Task<Stream> Get(Uri uri)
        {
            var info = _metaStorage.Get<MetaInfo>(uri);
            if (!_securityManager.MayRead(info))
                throw new SecurityException("No access to read");
            return await _fileStorage.Get(uri);
        }

        public Uri GetRedirectUri(Uri uri)
        {
            return _fileStorage.UriResolver.ResolveStaticUri(uri);
        }

        public async Task<MetaInfo> Create(Stream stream, IFileInfo fileInfo)
        {
            if (!_securityManager.MayCreate(fileInfo))
                throw new SecurityException("No access to create");
            fileInfo.Uri = await _fileStorage.Add(stream, fileInfo);
            var metaInfo = BuildMetadata(fileInfo);
            _metaStorage.Add(metaInfo);
            return metaInfo;
        }

        public async Task Delete(Uri uri)
        {
            var info = _metaStorage.Get<MetaInfo>(uri);
            if (!_securityManager.MayDelete(info))
                throw new SecurityException("No access to delete");

            await _fileStorage.Delete(uri);
            _metaStorage.Delete(uri);
        }

        public T GetInfo<T>(Uri uri) where T : MetaInfo
        {
            var res = _metaStorage.Get<T>(uri);
            if (!_securityManager.MayRead(res))
                throw new SecurityException("No access to read");

            return res;
        }

        public  Task UpdateMetadata(MetaInfo fileInfo)
        {
            _metaStorage.Add(fileInfo);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Override to build & store your own MetaInfo format
        /// </summary>
        /// <param name="uri">URI of stored file</param>
        /// <param name="fileInfo">Income file info</param>
        /// <returns></returns>
        protected virtual MetaInfo BuildMetadata(IFileInfo fileInfo)
        {
            return new MetaInfo(fileInfo)
            {
                StoragePath = fileInfo.Uri.AbsolutePath
            };
        }
    }
}