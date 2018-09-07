using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using Cactus.Fileserver.Logging;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Pipeline;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Middleware
{
    public class AddFileHandler
    {
        private static readonly ILog Log = LogProvider.GetLogger(typeof(AddFileHandler));
        private readonly FileProcessorDelegate _processPipelineEntry;

        public AddFileHandler(RequestDelegate next, FileProcessorDelegate processFunc)
        {
            _processPipelineEntry = processFunc;
            Log.Debug(".ctor");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var streamContent = new StreamContent(context.Request.Body);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);

            var meta = await AddFile(context, streamContent);
            context.Response.StatusCode = (int)HttpStatusCode.Created;
            context.Response.Headers.Add("Location", meta.Uri.ToString());
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(BuldOkResponseObject(meta)));
            Log.Info("Served by AddFileMiddleware");
        }

        protected virtual object BuldOkResponseObject(IFileInfo meta)
        {
            var metaCopy = new MetaInfo(meta) { StoragePath = null };
            return metaCopy;
        }

        protected virtual async Task<MetaInfo> AddFile(HttpContext context, HttpContent newFileContent)
        {
            return await _processPipelineEntry(context.Request, newFileContent, null,
                new MetaInfo { Owner = GetOwner(context.User?.Identity) });
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