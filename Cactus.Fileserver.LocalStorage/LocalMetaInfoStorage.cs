using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalMetaInfoStorage : IMetaInfoStorage
    {
        private readonly string _baseFolder;
        private readonly string _metafileExt;
        private readonly ILogger<LocalMetaInfoStorage> _log;

        public LocalMetaInfoStorage(IOptions<LocalMetaStorageOptions> settings, ILogger<LocalMetaInfoStorage> log)
        {
            _log = log;
            _baseFolder = settings.Value.BaseFolder;
            _metafileExt = settings.Value.Extension;
        }

        public async Task Add<T>(T info) where T : IMetaInfo
        {
            var fullFilename = GetFile(info.Uri);
            _log.LogDebug("Write metainfo to {file}", fullFilename);
            using (var writer = new StreamWriter(File.Create(fullFilename)))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(info));
            }
        }

        public Task Update<T>(T info) where T : IMetaInfo
        {
            return Add(info);
        }

        public Task Delete(Uri uri)
        {
            var fullFilename = GetFile(uri);
            _log.LogDebug("Delete metainfo {file}", fullFilename);
            File.Delete(fullFilename);
            return Task.CompletedTask;
        }

        public async Task<T> Get<T>(Uri uri) where T : IMetaInfo
        {
            var file = GetFile(uri);
            _log.LogDebug("Get metainfo from {file}", file);
            using (var reader = new StreamReader(new FileStream(file, FileMode.Open)))
            {
                return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
            }
        }

        /// <summary>
        /// Returns full file path to the metainfo file
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        protected virtual string GetFile(Uri uri)
        {
            var fileName = uri.GetResource();
            if (!string.IsNullOrWhiteSpace(_metafileExt)) fileName += _metafileExt;
            return Path.Combine(_baseFolder, fileName);
        }
    }
}