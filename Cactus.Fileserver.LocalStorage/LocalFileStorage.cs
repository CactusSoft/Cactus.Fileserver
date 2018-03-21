using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Logging;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileStorage<T> : BaseFileStore<T> where T: MetaInfo, new()
    {
        private const int MaxTriesCount = 10;

        private static readonly ILog Log =
            LogProvider.GetLogger(typeof(LocalFileStorage<>).Namespace + '.' + nameof(LocalFileStorage<T>));

        private readonly IStoredNameProvider<T> _nameProvider;

        public LocalFileStorage(Uri baseUri, IStoredNameProvider<T> nameProvider, string storeFolder = null):base(new DefaultUriResolver(baseUri, storeFolder))
        {
            _nameProvider = nameProvider;
        }

        public LocalFileStorage(IStoredNameProvider<T> nameProvider, IUriResolver uriResolver) : base(uriResolver)
        {
            _nameProvider = nameProvider;
        }


        protected override async Task<string> ExecuteAdd(Stream stream, T info)
        {
            var filename = _nameProvider.GetName(info);
            var fullFilePath = UriResolver.ResolvePath(filename);
            var triesCount = 0;
            for (; File.Exists(fullFilePath) && triesCount < MaxTriesCount; triesCount++)
            {
                filename = _nameProvider.Regenerate(info, filename);
                fullFilePath = UriResolver.ResolvePath(filename);
            }

            if (triesCount > MaxTriesCount)
                throw new IOException("Could not generate unique file name");

            using (var dest = new FileStream(fullFilePath, FileMode.CreateNew))
            {
                await stream.CopyToAsync(dest);
            }

            return filename;
        }

        public override Task Delete(Uri uri)
        {
            var file = UriResolver.ResolveFilename(uri);
            File.Delete(file);
            return Task.FromResult(0);
        }

        protected override Task<Stream> ExecuteGet(string filename)
        {
            var fullFilePath = UriResolver.ResolvePath(filename);
            var stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((Stream)stream);
        }

        protected class DefaultUriResolver : IUriResolver
        {
            private readonly string _baseFolder;
            private readonly Uri _baseUri;


            public DefaultUriResolver(Uri baseUri, string baseFolder)
            {
                _baseUri = baseUri;
                _baseFolder = Path.GetTempPath();
                if (!string.IsNullOrEmpty(baseFolder))
                    try
                    {
                        if (!Directory.Exists(baseFolder))
                            Directory.CreateDirectory(baseFolder);

                        _baseFolder = baseFolder;
                        Log.Info("Storage folder is configured successfully");
                    }
                    catch (Exception)
                    {
                        Log.ErrorFormat(
                            "Configuration error. StorageFolder {0} is unaccesable, temporary folder {1} will be used instead",
                            baseFolder, _baseFolder);
                    }
            }

            public Uri ResolveUri(string newFileName)
            {
                return new Uri(_baseUri, newFileName);
            }

            public string ResolveFilename(Uri fileUri)
            {
                return fileUri.GetResource();
            }

            public string ResolvePath(Uri fileUri)
            {
                return ResolvePath(fileUri.GetResource());
            }

            public string ResolvePath(string fileName)
            {
                return Path.Combine(_baseFolder, fileName);
            }
        }
    }
}