using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;

namespace Cactus.Fileserver
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IMetaInfoStorage _metaStorage;

        public FileStorageService(IMetaInfoStorage metaStorage, IFileStorage fileStorage)
        {
            _metaStorage = metaStorage;
            _fileStorage = fileStorage;
        }

        public async Task<Stream> Get(Uri uri)
        {
            var info = await _metaStorage.Get<MetaInfo>(uri);
            return await _fileStorage.Get(info);
        }

        public async Task Create(Stream stream, IMetaInfo metaInfo)
        {
            metaInfo.Uri = await _fileStorage.Add(stream, metaInfo);
            await _metaStorage.Add(metaInfo);
        }

        public async Task Delete(Uri uri)
        {
            var info = await _metaStorage.Get<MetaInfo>(uri);
            await Task.WhenAll(
                _fileStorage.Delete(info),
                _metaStorage.Delete(uri)
            );
        }

        public Task<T> GetInfo<T>(Uri uri) where T : IMetaInfo
        {
            return _metaStorage.Get<T>(uri);
        }

        public Task UpdateInfo(IMetaInfo fileInfo)
        {
            return _metaStorage.Update(fileInfo);
        }
    }
}