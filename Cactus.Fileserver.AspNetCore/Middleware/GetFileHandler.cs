using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProcessFunc = System.Func<Microsoft.AspNetCore.Http.HttpRequest, System.IO.Stream, System.Threading.Tasks.Task>;

namespace Cactus.Fileserver.AspNetCore.Middleware
{
    internal class GetFileHandler
    {
        private readonly ILogger log;
        protected readonly ProcessFunc ProcessFunc;

        public GetFileHandler(ILoggerFactory logFactory, ProcessFunc processFunc)
        {
            log = logFactory?.CreateLogger(GetType().Name);
            ProcessFunc = processFunc;
            log?.LogDebug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                using (var stream = context.Response.Body)
                {
                    await ProcessFunc.Invoke(context.Request, stream);
                }
            }
            catch
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
            }
        }
    }
}