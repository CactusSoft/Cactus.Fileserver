using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;

namespace Cactus.Fileserver
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
        ///     Store a new file from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="metaInfo"></param>
        /// <returns></returns>
        Task Create(Stream stream, IMetaInfo metaInfo);

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
        Task<T> GetInfo<T>(Uri uri) where T : IMetaInfo;

        /// <summary>
        ///  Update metadata
        /// </summary>
        /// <param name="fileInfo">Meta</param>
        /// <returns></returns>
        Task UpdateInfo(IMetaInfo fileInfo);

    }
}