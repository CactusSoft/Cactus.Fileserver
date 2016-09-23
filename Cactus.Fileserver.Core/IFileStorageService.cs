using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Get file content by URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<Stream> Get(Uri uri);

        /// <summary>
        /// Store a new file from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        Task<Uri> Create(Stream stream, IFileInfo fileInfo);

        /// <summary>
        /// Delete file by URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task Delete(Uri uri);

        /// <summary>
        /// Get file information
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IFileInfo GetInfo(Uri uri);
    }
}
