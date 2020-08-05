using System;
using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.LocalStorage
{
    public interface IUriResolver
    {
        /// <summary>
        /// Returns externally accessible uri to GET the file
        /// </summary>
        /// <returns></returns>
        Uri ResolveUri(IMetaInfo info);

        /// <summary>
        /// Returns path to a folder where the file is or where it should be added to
        /// </summary>
        /// <returns></returns>
        string ResolvePath(IMetaInfo info);
    }
}