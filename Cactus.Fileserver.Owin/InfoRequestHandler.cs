using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Owin.Config;
using Microsoft.Owin;

namespace Cactus.Fileserver.Owin
{
    public class InfoRequestHandler
    {
        private readonly IFileStorageService storageService;

        public InfoRequestHandler(IFileStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task Handle(IOwinContext context)
        {
            if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                var path = context.Request.Path.ToUriComponent();
                var infoSegmentIndex = path.LastIndexOf(AppBuilderExtensions.InfoPathSegment, StringComparison.OrdinalIgnoreCase);
                if (infoSegmentIndex > 0)
                {
                    path = path.Substring(0, infoSegmentIndex);
                }

                var uri = string.Concat(
                        context.Request.Scheme,
                        "://",
                        context.Request.Host.ToUriComponent(),
                        context.Request.PathBase.ToUriComponent(),
                        path,
                        context.Request.QueryString.ToUriComponent());

                var info = storageService.GetInfo(new Uri(uri));
                await context.Response.ResponseOk(info);
            }
            else
            {
                Trace.TraceWarning("Unsupported method");
                context.Response.StatusCode = 405;
            }
        }
    }
}

