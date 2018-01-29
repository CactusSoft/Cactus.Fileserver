using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Config
{
    public interface IFileserverConfig<T>
    {
        string Path { get; }

        Func<IFileStorageService> FileStorage { get; }

        Func<Func<T, HttpContent, IFileInfo, Task<MetaInfo>>> NewFilePipeline { get; }

        Func<Func<T, Task<Stream>>> GetFilePipeline { get; }

    }
}