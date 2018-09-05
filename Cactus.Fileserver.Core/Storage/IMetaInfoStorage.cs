using System;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    /// <summary>
    /// Storage of a file meta information.
    /// The meta information includes file its type, original name, owner and some other useful information.
    /// </summary>
    public interface IMetaInfoStorage
    {
        /// <summary>
        /// Put meta information to the storage
        /// </summary>
        /// <param name="info">Meta information.
        /// The Url field must be filled up - it uses to address the meta info in Get or Delete operations </param>
        void Add(MetaInfo info);

        /// <summary>
        /// Delete meta information by the file URI
        /// </summary>
        /// <param name="uri">A file URI. The same URI that was set in MetaInfo.Uri during Add operation</param>
        void Delete(Uri uri);

        /// <summary>
        /// Get a file metadata
        /// </summary>
        /// <typeparam name="T">Metadata could be deserialized as any other type derived from MetaInfo</typeparam>
        /// <param name="uri">A file URI. The same URI that was set in MetaInfo.Uri during Add operation</param>
        /// <returns>Meta data</returns>
        T Get<T>(Uri uri) where T : MetaInfo;
    }
}