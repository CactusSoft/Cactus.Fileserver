using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using ProcessFunc = System.Func<Microsoft.Owin.IOwinRequest, System.Net.Http.HttpContent, Cactus.Fileserver.Core.Model.IFileInfo, System.Threading.Tasks.Task<Cactus.Fileserver.Core.Model.MetaInfo>>;


namespace Cactus.Fileserver.Owin.Middleware
{
    public class AddFileMiddleware : OwinMiddleware
    {
        private readonly ProcessFunc processFunc;
        private readonly ILogger log;

        public AddFileMiddleware(OwinMiddleware next, ILoggerFactory logFactory, ProcessFunc processFunc) : base(next)
        {
            this.processFunc = processFunc;
            log = logFactory?.Create(GetType().Name);
            log?.WriteVerbose(".ctor");
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (HttpMethod.Post.Method.Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                var streamContent = new StreamContent(context.Request.Body);
                streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
                var meta = await AddFile(context, streamContent);
                context.Response.StatusCode = (int)HttpStatusCode.Created;
                context.Response.Headers.Add("Location", new[] { meta.Uri.ToString() });
                await context.Response.WriteAsync(JsonConvert.SerializeObject(BuldOkResponseObject(meta)));
                log?.WriteInformation("Served by AddFileMiddleware");
            }
            else await Next.Invoke(context);
        }

        protected virtual object BuldOkResponseObject(MetaInfo meta)
        {
            return new { meta.Uri, meta.Icon, meta.MimeType, meta.Extra };
        }

        protected virtual async Task<MetaInfo> AddFile(IOwinContext context, HttpContent newFileContent)
        {
            return await processFunc(context.Request, newFileContent, new IncomeFileInfo { Owner = GetOwner(context.Authentication?.User?.Identity) });
        }
        
        /// <summary>
        /// Returns a string that represent file owner based on authentication context.
        /// By default returns Identity.Name or nul if user is not authenticated.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>Owner as a string or null</returns>
        protected virtual string GetOwner(IIdentity identity)
        {
            return identity?.Name;
        }
    }
}
