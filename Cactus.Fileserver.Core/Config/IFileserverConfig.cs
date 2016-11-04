using System;
using System.Net.Http;

namespace Cactus.Fileserver.Core.Config
{
    public interface IFileserverConfig<T>
    {
        string Path { get; }

        Func<IFileStorageService> FileStorage { get; }

        Func<Func<T, HttpContent, Model.IFileInfo, System.Threading.Tasks.Task<Model.MetaInfo>>> NewFilePipeline { get; }
    }
}