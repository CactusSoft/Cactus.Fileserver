using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace Cactus.Fileserver.Owin.Middleware
{
    public class DeleteFileMiddleware<T> : OwinMiddleware where T : IFileInfo
    {
        private readonly ILogger log;
        protected readonly IFileStorageService<T> StorageService;

        public DeleteFileMiddleware(OwinMiddleware next, ILoggerFactory logFactory, IFileStorageService<T> storageService) : base(next)
        {
            log = logFactory?.Create(GetType().Name);
            StorageService = storageService;
            log?.WriteVerbose(".ctor");
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (HttpMethod.Delete.Method.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                await StorageService.Delete(context.Request.Uri);
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                log?.WriteInformation("Served by DeleteFileMiddleware");
            }
            else await Next.Invoke(context);
        }
    }
}
