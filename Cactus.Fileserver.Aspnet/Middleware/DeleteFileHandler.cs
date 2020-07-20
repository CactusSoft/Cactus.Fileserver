using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.Aspnet.Middleware
{
    internal class DeleteFileHandler
    {
        protected readonly IFileStorageService StorageService;
        private readonly ILogger<AddFileHandler> _log;

        public DeleteFileHandler(RequestDelegate next, IFileStorageService storageService, ILogger<AddFileHandler> log)
        {
            StorageService = storageService;
            _log = log;
            log.LogDebug(".ctor");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await StorageService.Delete(context.Request.GetAbsoluteUri());
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            _log.LogInformation("Served by {handler}", nameof(DeleteFileHandler));
        }
    }
}