using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cactus.Fileserver.Asp5.Config;
using Cactus.Fileserver.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Asp5
{
    public class InfoRequestHandler
    {
        private readonly IFileStorageService storageService;

        public InfoRequestHandler(IFileStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task Handle(HttpContext context)
        {
            if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var path = context.Request.Path.ToUriComponent();
                path = path.Substring(0, path.LastIndexOf(AppBuilderExtension.InfoPathSegment));
                var uri = string.Concat(
                        context.Request.Scheme,
                        "://",
                        context.Request.Host.ToUriComponent(),
                        context.Request.PathBase.ToUriComponent(),
                        path,
                        context.Request.QueryString.ToUriComponent());

                var info = storageService.GetInfo(new Uri(uri));
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(info));
            }
            else
            {
                Trace.TraceWarning("Unsupported method");
                context.Response.StatusCode = 405;
            }
        }
    }
}

