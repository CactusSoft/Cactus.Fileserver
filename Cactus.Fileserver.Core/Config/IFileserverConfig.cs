using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Config
{
    public interface IFileserverConfig<in TRequest, TMeta> where TMeta : IFileInfo
    {
        string Path { get; }

        Func<IFileStorageService<TMeta>> FileStorage { get; }

        Func<Func<TRequest, HttpContent, TMeta, Task<TMeta>>> NewFilePipeline { get; }

        Func<Func<TRequest, IFileGetContext<TMeta>, Task>> GetFilePipeline { get; }

    }
}