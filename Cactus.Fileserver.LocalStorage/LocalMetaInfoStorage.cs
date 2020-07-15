using System;
using System.IO;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalMetaInfoStorage : IMetaInfoStorage
    {
        private readonly string _baseFolder;
        private readonly string _metafileExt;
        private readonly ILogger _log;

        public LocalMetaInfoStorage(string folder, ILogger log, string fileExt = ".json")
        {
            _log = log;
            if (string.IsNullOrEmpty(fileExt))
                throw new ArgumentNullException(nameof(fileExt));
            if (fileExt[0] != '.')
                throw new ArgumentException("File extension should be started from dot symbol", nameof(fileExt));
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException(nameof(folder));

            _metafileExt = fileExt;
            _log = log;
            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                _baseFolder = folder;
                _log.LogInformation("Storage folder is configured successfully");
            }
            catch (Exception)
            {
                _log.LogError(
                    "Configuration error. StorageFolder {0} is inaccessible, temporary folder {1} will be used instead",
                    folder, _baseFolder);
            }
        }

        public void Add(MetaInfo info)
        {
            var fullFilename = GetFile(info.Uri);
            using (var writer = new StreamWriter(File.Create(fullFilename)))
            {
                // Damn XMLSerializer could not serialize Uri type, cause of it has no default constructor. What is the bullshit!!!!
                // Use JSON and relax.

                writer.Write(JsonConvert.SerializeObject(info));
            }
        }

        public void Delete(Uri uri)
        {
            var fullFilename = GetFile(uri);
            File.Delete(fullFilename);
        }

        public T Get<T>(Uri uri) where T : MetaInfo
        {
            var metafile = GetFile(uri);
            using (var reader = new StreamReader(new FileStream(metafile, FileMode.Open)))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }

        protected string GetFile(Uri uri)
        {
            return Path.Combine(_baseFolder, uri.GetResource() + _metafileExt);
        }
    }
}