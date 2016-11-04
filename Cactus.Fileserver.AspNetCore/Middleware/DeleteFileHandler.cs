using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.AspNetCore.Middleware
{
    class DeleteFileHandler
    {
        private readonly ILogger log;
        protected readonly IFileStorageService StorageService;

        public DeleteFileHandler(ILoggerFactory logFactory, IFileStorageService storageService)
        {
            log = logFactory?.CreateLogger(GetType().Name);

            StorageService = storageService;
            log?.LogDebug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            await StorageService.Delete(context.Request.GetAbsoluteUri());
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            log?.LogInformation("Served by DeleteFileMiddleware");
        }
    }
}
