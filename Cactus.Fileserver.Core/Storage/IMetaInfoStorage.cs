using System;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IMetaInfoStorage<T> where T : IFileInfo
    {
        void Add(T info);

        void Delete(Uri uri);

        T Get(Uri uri);

    }
}