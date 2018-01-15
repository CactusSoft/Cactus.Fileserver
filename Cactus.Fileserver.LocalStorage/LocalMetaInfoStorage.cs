using System;
using System.IO;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Logging;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;
using Newtonsoft.Json;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalMetaInfoStorage<T> : IMetaInfoStorage<T> where T : MetaInfo, new()
    {
        private static readonly ILog Log =
            LogProvider.GetLogger(typeof(LocalMetaInfoStorage<>).Namespace + '.' + nameof(LocalMetaInfoStorage<T>));

        private readonly string baseFolder;
        private readonly string metafileExt;

        public LocalMetaInfoStorage(string folder, string fileExt = ".json")
        {
            if (string.IsNullOrEmpty(fileExt))
                throw new ArgumentNullException(nameof(fileExt));
            if (fileExt[0] != '.')
                throw new ArgumentException("File extension shild be started from dot symbol", nameof(fileExt));
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException(nameof(folder));

            metafileExt = fileExt;
            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                baseFolder = folder;
                Log.Info("Storage folder is configured successfully");
            }
            catch (Exception)
            {
                Log.ErrorFormat(
                    "Configuration error. StorageFolder {0} is unaccesable, temporary folder {1} will be used instead",
                    folder, baseFolder);
            }
        }

        public void Add(T info)
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

        public T Get(Uri uri)
        {
            var metafile = GetFile(uri);
            return GetMetadata(metafile);
        }

        protected string GetFile(Uri uri)
        {
            return Path.Combine(baseFolder, uri.GetResource() + metafileExt);
        }

        protected T GetMetadata(string metafile)
        {
            using (var reader = new StreamReader(new FileStream(metafile, FileMode.Open)))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }
    }
}