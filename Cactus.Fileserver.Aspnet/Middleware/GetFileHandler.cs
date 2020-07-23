using System;
using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.Aspnet.Middleware
{
    public class GetFileHandler
    {
        private readonly ILogger<GetFileHandler> _log;

        public GetFileHandler(RequestDelegate next, ILogger<GetFileHandler> log)
        {
            _log = log;
            log.LogDebug(".ctor");
        }

        public async Task InvokeAsync(HttpContext ctx, IFileStorageService storageService)
        {
            var meta = await storageService.GetInfo<MetaInfo>(ctx.Request.GetAbsoluteUri());
            var contentTask = storageService.Get(ctx.Request.GetAbsoluteUri());
            ctx.Response.StatusCode = (int)HttpStatusCode.OK;
            ctx.Response.ContentType = meta.MimeType;
            if (meta.OriginalName != null)
                ctx.Response.Headers.Add("Content-Disposition", $"attachment;filename=UTF-8''{Uri.EscapeDataString(meta.OriginalName)}");
            await (await contentTask).CopyToAsync(ctx.Response.Body);
            _log.LogInformation("Served by {handler}", nameof(DeleteFileHandler));
        }
    }
}