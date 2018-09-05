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
        /// Returns full URI to a file by a file name.
        /// The URI could be used to get, delete the file itself or its metadata.
        /// </summary>
        /// <param name="newFileName">New file name</param>
        /// <returns>Full URI to a file</returns>
        Uri ResolveUri(string newFileName);

        /// <summary>
        /// Extracts a filename from the full URI. 
        /// </summary>
        /// <param name="fileUri">File URI</param>
        /// <returns>File name</returns>
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