using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    /// <summary>
    /// Organize access to file based on URI
    /// </summary>
    public interface IFileStorage
    { 
        IUriResolver UriResolver { get; }

        /// <summary>
        /// Add file to the storage
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="info">File info.
        /// The meta information that could be used in some specific storage implementations like S3 or AzureBlob </param>
        /// <returns>An URI that can be used to Get or Delete operations</returns>
        Task<Uri> Add(Stream stream, IFileInfo info);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        /// <param name="uri">URI received from Create method</param>
        /// <returns></returns>
        Task Delete(Uri uri);

        /// <summary>
        /// Get file content
        /// </summary>
        /// <param name="uri">URI received from Create method</param>
        /// <returns>File content as a stream</returns>
        Task<Stream> Get(Uri uri);
    }
}