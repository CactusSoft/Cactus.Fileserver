using System;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage
{
    /// <summary>
    /// Direct all requests to store files into base folder 
    /// </summary>
    public class SingleFolderUriResolver : IUriResolver
    {
        private readonly string _baseFolder;
        private readonly string _baseUri;

        public SingleFolderUriResolver(IOptions<LocalFileStorageOptions> settings)
        {
            _baseUri = settings.Value.BaseUri.ToString().TrimEnd('/');
            _baseFolder = settings.Value.BaseFolder;
        }

        public Uri ResolveUri(IMetaInfo info)
        {
            _ = info?.InternalUri ?? throw new ArgumentNullException(nameof(IMetaInfo) + '.' + nameof(IMetaInfo.InternalUri));
            var fileName = info.InternalUri.GetResource();
            return new Uri(_baseUri + '/' + fileName);
        }

        public string ResolvePath(IMetaInfo info)
        {
            return _baseFolder;
        }
    }
}