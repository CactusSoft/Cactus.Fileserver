using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.AspNetCore.Middleware
{
    internal class GetFileHandler<T> where T : class, IFileInfo, new()
    {
        private readonly ILogger log;
        protected readonly Func<HttpRequest, IFileGetContext<T>, Task> ProcessFunc;

        public GetFileHandler(ILoggerFactory logFactory, Func<HttpRequest, IFileGetContext<T>, Task> processFunc)
        {
            log = logFactory?.CreateLogger(GetType().Name);
            ProcessFunc = processFunc;
            log?.LogDebug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            var getContext = new FileGetContext<T>();
            try
            {
                using (var stream = new MemoryStream())
                {
                    getContext.ContextStream = stream;
                    await ProcessFunc.Invoke(context.Request, getContext);
                    if (getContext.RedirectUri != null && getContext.IsNeedToRedirect)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.MovedPermanently;
                        context.Response.Headers.Append("Location", getContext.RedirectUri.ToString());
                    }
                    else if (getContext.ContextStream?.CanRead == true && getContext.IsNeedToPromoteStream)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        getContext.ContextStream.Position = 0;
                        await getContext.ContextStream.CopyToAsync(context.Response.Body);
                    }
                    else
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                    }
                }
            }
            catch
            {
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
            }
        }
    }
}