using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.Core
{
    public class FileStorageService<T> : IFileStorageService where T : MetaInfo, new()
    {
        private readonly IMetaInfoStorage<T> metaStorage;
        private readonly IFileStorage<T> fileStorage;
        private readonly ISecurityManager securityManager;

        public FileStorageService(IMetaInfoStorage<T> metaStorage, IFileStorage<T> fileStorage, ISecurityManager securityManager)
        {
            this.metaStorage = metaStorage;
            this.fileStorage = fileStorage;
            this.securityManager = securityManager;
        }

        public async Task<Stream> Get(Uri uri)
        {
            var info = metaStorage.Get(uri);
            if (!securityManager.MayRead(info))
            {
                throw new AccessViolationException("No access to delete");
            }
            return await fileStorage.Get(uri);
        }

        public async Task<Uri> Create(Stream stream, IFileInfo fileInfo)
        {
            if (!securityManager.MayCreate(fileInfo))
            {
                throw new AccessViolationException("No access to delete");
            }

            T meta = new T
            {
                MimeType = fileInfo.MimeType,
                Name = fileInfo.Name,
                Owner = fileInfo.Owner,
                Size = fileInfo.Size
            };
            meta.Uri = await fileStorage.Add(stream, meta);
            metaStorage.Add(meta);
            return meta.Uri;
        }

        public async Task Delete(Uri uri)
        {
            var info = metaStorage.Get(uri);
            if (!securityManager.MayDelete(info))
            {
                throw new AccessViolationException("No access to delete");
            }

            await fileStorage.Delete(uri);
            metaStorage.Delete(uri);
        }

        public IFileInfo GetInfo(Uri uri)
        {
            return metaStorage.Get(uri);
        }
    }
}