using System;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage
{
    public interface IUriResolver
    {
        /// <summary>
        /// Returns uri to GET the file
        /// </summary>
        /// <returns></returns>
        Uri ResolveUri(IMetaInfo info);

        /// <summary>
        /// Returns path to a folder where the file is or should be added to
        /// </summary>
        /// <returns></returns>
        string ResolvePath(IMetaInfo info);
    }

    /// <summary>
    /// Direct all requests to store files into base folder 
    /// </summary>
    public class BaseFolderUriResolver : IUriResolver
    {
        private readonly string _baseFolder;
        private readonly string _baseUri;

        public BaseFolderUriResolver(IOptions<LocalFileStorageOptions> settings)
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