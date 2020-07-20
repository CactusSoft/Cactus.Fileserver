using System;
using Cactus.Fileserver.Model;

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

    public class AllInTheSameFolderUriResolver : IUriResolver
    {
        private readonly string _baseFolder;
        private readonly string _baseUri;

        public AllInTheSameFolderUriResolver(Uri baseUri, string baseFolder)
        {
            _baseUri = baseUri.ToString().TrimEnd('/');
            _baseFolder = baseFolder;
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