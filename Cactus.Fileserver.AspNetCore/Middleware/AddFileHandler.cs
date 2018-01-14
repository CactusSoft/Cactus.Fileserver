using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProcessFunc =
    System.Func<Microsoft.AspNetCore.Http.HttpRequest, System.Net.Http.HttpContent,
        Cactus.Fileserver.Core.Model.IFileInfo, System.Threading.Tasks.Task<Cactus.Fileserver.Core.Model.MetaInfo>>;


namespace Cactus.Fileserver.AspNetCore.Middleware
{
    public class AddFileHandler
    {
        private readonly ILogger log;
        private readonly ProcessFunc processFunc;

        public AddFileHandler(ILoggerFactory logFactory, ProcessFunc processFunc)
        {
            this.processFunc = processFunc;
            log = logFactory?.CreateLogger(GetType().Name);
            log?.LogDebug(".ctor");
        }

        public async Task Invoke(HttpContext context)
        {
            var streamContent = new StreamContent(context.Request.Body);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);

            var meta = await AddFile(context, streamContent);
            context.Response.StatusCode = (int) HttpStatusCode.Created;
            context.Response.Headers.Add("Location", meta.Uri.ToString());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(BuldOkResponseObject(meta)));
            log?.LogInformation("Served by AddFileMiddleware");
        }

        protected virtual object BuldOkResponseObject(MetaInfo meta)
        {
            return new {meta.Uri, meta.Icon, meta.MimeType, meta.Extra};
        }

        protected virtual async Task<MetaInfo> AddFile(HttpContext context, HttpContent newFileContent)
        {
            return await processFunc(context.Request, newFileContent,
                new IncomeFileInfo {Owner = GetOwner(context.User?.Identity)});
        }

        /// <summary>
        ///     Try to extract original file name from the request
        /// </summary>
        /// <param name="contentHeaders"></param>
        /// <returns>Returns empty string if nothing found</returns>
        protected virtual string GetOriginalFileName(HttpContentHeaders contentHeaders)
        {
            return contentHeaders.ContentDisposition.FileName?.Trim('"') ?? "anonymous";
        }

        /// <summary>
        ///     Returns a string that represent file owner based on authentication context.
        ///     By default returns Identity.Name or nul if user is not authenticated.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>Owner as a string or null</returns>
        protected virtual string GetOwner(IIdentity identity)
        {
            return identity?.Name;
        }

        /// <summary>
        ///     Returns file info extracted from the request.
        ///     Good point to add extra fields or override some of them
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="contentHeaders"></param>
        /// <returns></returns>
        protected virtual IFileInfo SetFileInfo(IFileInfo fileInfo, HttpContentHeaders contentHeaders)
        {
            return new IncomeFileInfo
            {
                MimeType = contentHeaders.ContentType.ToString(),
                OriginalName = GetOriginalFileName(contentHeaders)
            };
        }
    }
}