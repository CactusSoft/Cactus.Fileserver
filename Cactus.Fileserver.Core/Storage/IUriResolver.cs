using System;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IUriResolver
    {
        /// <summary>
        /// Resolves static storage uri
        /// </summary>
        /// <param name="currentUri">Fileserver HTTP request uri</param>
        /// <returns></returns>
        Uri ResolveStaticUri(Uri currentUri);

        /// <summary>
        /// Resolve fileserver HTTP request uri
        /// </summary>
        /// <param name="newFileName">New file name</param>
        /// <returns></returns>
        Uri ResolveUri(string newFileName);

        /// <summary>
        /// Resolve filename
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        string ResolveFilename(Uri fileUri);

        /// <summary>
        /// Resolve path
        /// </summary>
        /// <param name="fileUri">Fileserver HTTP request uri</param>
        /// <returns></returns>
        string ResolvePath(Uri fileUri);

        /// <summary>
        /// Resolve path
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        string ResolvePath(string fileName);


    }
}