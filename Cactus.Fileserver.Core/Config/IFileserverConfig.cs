using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Core.Config
{
    public interface IFileserverConfig<TMeta> where TMeta : IFileInfo
    {
        string Path { get; }

        Func<IFileStorageService<TMeta>> FileStorage { get; }

        Func<Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>> NewFilePipeline { get; }

        IApplicationBuilder GetPipeline(IApplicationBuilder app);
    }
}