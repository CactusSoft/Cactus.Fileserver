using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public interface IFileStorageService
    {
        /// <summary>
        ///     Get file content by URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<Stream> Get(Uri uri);

        /// <summary>
        ///  Get uri to static file
        /// </summary>
        /// <param name="uri">Request Uri</param>
        /// <returns></returns>
        Uri GetRedirectUri(Uri uri);


        /// <summary>
        ///     Store a new file from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        Task<MetaInfo> Create(Stream stream, IFileInfo fileInfo);

        /// <summary>
        ///     Delete file by URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task Delete(Uri uri);

        /// <summary>
        ///     Get file information
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        T GetInfo<T>(Uri uri) where T : MetaInfo;

        /// <summary>
        ///  Update metadata
        /// </summary>
        /// <param name="fileInfo">Meta</param>
        /// <returns></returns>
        Task UpdateMetadata(MetaInfo fileInfo);

    }
}