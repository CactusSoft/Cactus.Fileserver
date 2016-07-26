using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IFileStorage<T> where T : MetaInfo, new()
    {
        Task<Uri> Add(Stream stream, T info);

        Task Delete(Uri uri);

        Task<Stream> Get(Uri uri);
    }
}
