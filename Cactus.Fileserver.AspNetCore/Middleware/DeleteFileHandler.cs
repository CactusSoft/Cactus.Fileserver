using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.AspNetCore.Middleware
{
    internal class DeleteFileHandler<T> where T : IFileInfo
    {
        private readonly ILogger log;
        protected readonly IFileStorageService<T> StorageService;

        public DeleteFileHandler(ILoggerFactory logFactory, IFileStorageService<T> storageService)
        {
            log = logFactory?.CreateLogger(GetType().Name);

            StorageService = storageService;
            log?.LogDebug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            await StorageService.Delete(context.Request.GetAbsoluteUri());
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            log?.LogInformation("Served by DeleteFileMiddleware");
        }
    }
}