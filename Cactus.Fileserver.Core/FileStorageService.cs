using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.Core
{
    public class FileStorageService<T> : IFileStorageService<T> where T : IFileInfo
    {
        private readonly IFileStorage<T> fileStorage;
        private readonly IMetaInfoStorage<T> metaStorage;
        private readonly ISecurityManager securityManager;

        public FileStorageService(IMetaInfoStorage<T> metaStorage, IFileStorage<T> fileStorage,
            ISecurityManager securityManager)
        {
            this.metaStorage = metaStorage;
            this.fileStorage = fileStorage;
            this.securityManager = securityManager;
        }

        public async Task<Stream> Get(Uri uri)
        {
            var info = metaStorage.Get(uri);
            if (!securityManager.MayRead(info))
                throw new SecurityException("No access to read");
            return await fileStorage.Get(uri);
        }

        public Uri GetRedirectUri(Uri uri)
        {
             return fileStorage.UriResolver.ResolveStaticUri(uri);
        }

        public async Task<T> Create(Stream stream, T fileInfo)
        {
            if (!securityManager.MayCreate(fileInfo))
                throw new SecurityException("No access to create");


            fileInfo.Uri = await fileStorage.Add(stream, fileInfo);
            fileInfo.StoragePath = fileInfo.Uri.AbsolutePath;
            metaStorage.Add(fileInfo);
            return fileInfo;
        }

        public async Task Delete(Uri uri)
        {
            var info = metaStorage.Get(uri);
            if (!securityManager.MayDelete(info))
                throw new SecurityException("No access to delete");

            await fileStorage.Delete(uri);
            metaStorage.Delete(uri);
        }

        public T GetInfo(Uri uri)
        {
            var res = metaStorage.Get(uri);
            if (!securityManager.MayRead(res))
                throw new SecurityException("No access to read");

            return res;
        }

        public Task UpdateMetadata(T fileInfo)
        {
            metaStorage.Add(fileInfo);
            return Task.FromResult(0);
        }
    }
}