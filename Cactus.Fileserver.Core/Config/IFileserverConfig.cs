using System;
using Cactus.Fileserver.AspNetCore;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Core.Config
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