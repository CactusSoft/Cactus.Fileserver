using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Owin.Config;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace Cactus.Fileserver.Owin
{
    public class InfoRequestHandler
    {
        private readonly ILogger log;
        private readonly IFileStorageService storageService;

        public InfoRequestHandler(ILoggerFactory logFactory, IFileStorageService storageService)
        {
            log = logFactory.Create(typeof(InfoRequestHandler).FullName);
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
                log.WriteWarning("Unsupported method {0}", context.Request.Method);
                Trace.TraceWarning("Unsupported method");
                context.Response.StatusCode = 405;
            }
        }
    }
}

