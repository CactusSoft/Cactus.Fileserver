using System;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IMetaInfoStorage
    {
        void Add(MetaInfo info);

        void Delete(Uri uri);

        T Get<T>(Uri uri) where T : MetaInfo;
    }
}