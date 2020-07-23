using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.Aspnet.Middleware
{
    public class DeleteFileHandler
    {
        private readonly ILogger<DeleteFileHandler> _log;

        public DeleteFileHandler(RequestDelegate next, ILogger<DeleteFileHandler> log)
        {
            _log = log;
            log.LogDebug(".ctor");
        }

        public async Task InvokeAsync(HttpContext context, IFileStorageService storageService)
        {
            await storageService.Delete(context.Request.GetAbsoluteUri());
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            _log.LogInformation("Served by {handler}", nameof(DeleteFileHandler));
        }
    }
}