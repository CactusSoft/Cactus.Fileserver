using System;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Config
{
    public interface IFileserverConfig
    {
        string Path { get; }

        Func<IFileStorageService> FileStorage { get; }

        Func<FileProcessorDelegate> NewFilePipeline { get; }

        //IApplicationBuilder PostPipeline(IApplicationBuilder app);

        IApplicationBuilder GetPipeline(IApplicationBuilder app);
    }
}