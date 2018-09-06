using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Logging;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Middleware
{
    internal class DeleteFileHandler
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(DeleteFileHandler));
        protected readonly IFileStorageService StorageService;

        public DeleteFileHandler(IFileStorageService storageService)
        {
            StorageService = storageService;
            Log.Debug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            await StorageService.Delete(context.Request.GetAbsoluteUri());
            context.Response.StatusCode = (int) HttpStatusCode.NoContent;
            Log.Info("Served by DeleteFileMiddleware");
        }
    }
}