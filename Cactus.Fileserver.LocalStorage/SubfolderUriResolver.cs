using System;
using System.IO;
using System.Linq;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage
{
    /// <summary>
    /// Direct storing files in subfolders of the baseFolder based on upload URI
    /// </summary>
    public class SubfolderUriResolver : IUriResolver
    {
        private readonly string _baseFolder;
        private readonly string _baseUri;
        private static readonly char UriPathSeparator = '/';

        public SubfolderUriResolver(IOptions<LocalFileStorageOptions> settings)
        {
            _baseUri = settings.Value.BaseUri.ToString().TrimEnd(UriPathSeparator);
            _baseFolder = settings.Value.BaseFolder;
        }

        public Uri ResolveUri(IMetaInfo info)
        {
            _ = info?.InternalUri ?? throw new ArgumentNullException(nameof(IMetaInfo) + '.' + nameof(IMetaInfo.InternalUri));
            var path = info.InternalUri.AbsolutePath;
            var basePath = _baseFolder;
            if (Path.DirectorySeparatorChar != UriPathSeparator)
            {
                basePath = basePath.Replace(Path.DirectorySeparatorChar, UriPathSeparator);
            }
            if (path.StartsWith(basePath))
            {
                var subPath = path.Substring(_baseFolder.Length);
                var res = new Uri(_baseUri + UriPathSeparator + subPath.TrimStart(UriPathSeparator));
                return res;
            }

            return new Uri(_baseUri + UriPathSeparator + info.InternalUri.GetResource());
        }

        public string ResolvePath(IMetaInfo info)
        {
            _ = info?.Uri ?? throw new ArgumentNullException(nameof(IMetaInfo) + '.' + nameof(IMetaInfo.Uri));
            if (info.Uri.AbsolutePath != "/")
            {
                var path = info.Uri.AbsolutePath.Trim(UriPathSeparator)
                    .Split(UriPathSeparator)
                    .Aggregate(_baseFolder, Path.Combine);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
            return _baseFolder;
        }
    }
}