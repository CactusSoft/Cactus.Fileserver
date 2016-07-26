using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core
{
    public interface IFileStorageService
    {
        Task<Stream> Get(Uri uri);

        Task<Uri> Create(Stream stream, IFileInfo fileInfo);

        Task Delete(Uri uri);
        IFileInfo GetInfo(Uri uri);
    }
}
