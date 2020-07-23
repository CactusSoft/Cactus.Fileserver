using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.Aspnet.Middleware
{
    public class RedirectToInternalStorageHandler
    {
        private readonly ILogger<RedirectToInternalStorageHandler> _log;

        public RedirectToInternalStorageHandler(RequestDelegate next, ILogger<RedirectToInternalStorageHandler> log)
        {
            _log = log;
            log.LogDebug(".ctor");
        }

        public async Task InvokeAsync(HttpContext context, IFileStorageService storageService)
        {
            var info = await storageService.GetInfo<MetaInfo>(context.Request.GetAbsoluteUri());
            context.Response.Redirect(info.InternalUri.ToString(), true);
            _log.LogInformation("Served by {handler}", nameof(RedirectToInternalStorageHandler));
        }
    }
}