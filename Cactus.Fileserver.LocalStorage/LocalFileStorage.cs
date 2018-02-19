using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Logging;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileStorage<T> : IFileStorage<T> where T : MetaInfo, new()
    {
        private const int MaxTriesCount = 10;

        private static readonly ILog Log =
            LogProvider.GetLogger(typeof(LocalFileStorage<>).Namespace + '.' + nameof(LocalFileStorage<T>));

        private readonly string baseFolder;
        private readonly Uri baseUri;
        private readonly IStoredNameProvider<T> nameProvider;

        public LocalFileStorage(Uri baseUri, IStoredNameProvider<T> nameProvider, string storeFolder = null)
        {
            this.baseUri = baseUri;
            this.nameProvider = nameProvider;
            baseFolder = Path.GetTempPath();
            if (!string.IsNullOrEmpty(storeFolder))
                try
                {
                    if (!Directory.Exists(storeFolder))
                        Directory.CreateDirectory(storeFolder);

                    baseFolder = storeFolder;
                    Log.Info("Storage folder is configured successfully");
                }
                catch (Exception)
                {
                    Log.ErrorFormat(
                        "Configuration error. StorageFolder {0} is unaccesable, temporary folder {1} will be used instead",
                        storeFolder, baseFolder);
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
                throw new IOException("Could not generate unique file name");

            using (var dest = new FileStream(fullFilePath, FileMode.CreateNew))
            {
                await stream.CopyToAsync(dest);
            }

            return new Uri(baseUri, filename);
        }

        public Task Delete(Uri uri)
        {
            var file = GetFile(uri);
            File.Delete(file);
            return Task.CompletedTask;
        }

        public Task<Stream> Get(Uri uri)
        {
            var fullFilePath = GetFile(uri);
            var stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((Stream)stream);
        }

        protected string GetFile(Uri uri)
        {
            return Path.Combine(baseFolder, uri.GetResource());
        }
    }
}