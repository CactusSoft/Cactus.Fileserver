using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using log4net;

namespace Cactus.Fileserver.Core.Storage
{
    public class LocalFileStorage<T> : IFileStorage<T> where T : MetaInfo, new()
    {
        private readonly Uri baseUri;
        private readonly IStoredNameProvider<T> nameProvider;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalFileStorage<>).Namespace + '.' + nameof(LocalFileStorage<T>));
        private readonly string baseFolder;
        private const int MaxTriesCount = 10;

        public LocalFileStorage(string folder, Uri baseUri, IStoredNameProvider<T> nameProvider)
        {
            this.baseUri = baseUri;
            this.nameProvider = nameProvider;
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

        public async Task<Uri> Add(Stream stream, T info)
        {
            var filename = nameProvider.GetName(info);
            var fullFilePath = Path.Combine(baseFolder, filename);
            var triesCount = 0;
            for (; File.Exists(fullFilePath) && triesCount < MaxTriesCount; triesCount++)
            {
                filename = nameProvider.Regenerate(info, filename);
                fullFilePath = Path.Combine(baseFolder, filename);
            }

            if (triesCount > MaxTriesCount)
            {
                throw new IOException("Could not generate unique file name");
            }

            using (var dest = new FileStream(fullFilePath, FileMode.CreateNew))
            {
                await stream.CopyToAsync(dest);
            }

            return new Uri(baseUri, filename);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task Delete(Uri uri)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            File.Delete(uri.GetResource());
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<Stream> Get(Uri uri)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var fullFilePath = Path.Combine(baseFolder, uri.GetResource());
            var stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream;
        }
    }
}
