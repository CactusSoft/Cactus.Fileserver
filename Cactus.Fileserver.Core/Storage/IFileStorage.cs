using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IFileStorage
    { 
        IUriResolver UriResolver { get; }

        Task<Uri> Add(Stream stream, IFileInfo info);

        Task Delete(Uri uri);

        Task<Stream> Get(Uri uri);
    }
}