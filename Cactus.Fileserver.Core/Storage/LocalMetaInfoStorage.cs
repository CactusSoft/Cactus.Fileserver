using System;
using System.IO;
using Cactus.Fileserver.Core.Model;
using log4net;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Core.Storage
{
    public class LocalMetaInfoStorage<T> : IMetaInfoStorage<T> where T : MetaInfo, new()
    {
        private readonly string baseFolder;
        private const string MetafileExt = ".meta";
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalMetaInfoStorage<>).Namespace + '.' + nameof(LocalMetaInfoStorage<T>));

        public LocalMetaInfoStorage(string folder)
        {
            baseFolder = Path.GetTempPath();
            if (!string.IsNullOrEmpty(folder))
            {
                try
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    baseFolder = folder;
                    Log.Info("Storage folder is configured successfully");
                }
                catch (Exception)
                {
                    Log.ErrorFormat("Configuration error. StorageFolder {0} is unaccesable, temporary folder {1} will be used instead", folder, baseFolder);
                }
            }
        }

        public void Add(T info)
        {
            var fullFilename = GetFile(info.Uri);
            using (var writer = new StreamWriter(fullFilename))
            {
                // Damn XMLSerializer could not serialize Uri type, cause of it has no default constructor. What is the bullshit!!!!
                // Use JSON and relax.
                writer.WriteAsync(JsonConvert.SerializeObject(info, Formatting.Indented));
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
            return Path.Combine(baseFolder, uri.GetResource() + MetafileExt);
        }

        protected T GetMetadata(string metafile)
        {
            using (var stream = new FileStream(metafile, FileMode.Open))
            {
                var sr = new StreamReader(stream);
                return JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
            }
        }
    }
}
