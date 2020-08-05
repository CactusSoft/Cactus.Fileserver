using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Cactus.Fileserver.Aspnet.Dto;
using Cactus.Fileserver.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Aspnet.Middleware
{
    /// <summary>
    /// Terminal handler for adding file. Put the request body into a file regardless of the content
    /// </summary>
    public class AddFileHandler
    {
        protected static readonly string JsonMimeType = "application/json";
        private readonly ILogger<AddFileHandler> _log;

        public AddFileHandler(RequestDelegate next, ILogger<AddFileHandler> log)
        {
            _log = log;
        }

        public async Task InvokeAsync(HttpContext ctx, IFileStorageService fileStorage)
        {
            _log.LogDebug("Store request content {content-type} into a file", ctx.Request.ContentType);
            var metaInfo = await ProcessUpload(ctx, fileStorage);
            await ResponseForUpload(ctx, metaInfo);
            _log.LogInformation("Served by {handler}", GetType().Name);
        }

        private async Task ResponseForUpload(HttpContext ctx, IMetaInfo metaInfo)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Created;
            ctx.Response.Headers.Add("Location", metaInfo.Uri.ToString());
            ctx.Response.ContentType = JsonMimeType;
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(BuldOkResponseObject(metaInfo)));
        }

        protected virtual async Task<IMetaInfo> ProcessUpload(HttpContext ctx, IFileStorageService fileStorage)
        {
            var meta = BuildMetaInfo(ctx);
            await fileStorage.Create(ctx.Request.Body, meta);
            return meta;
        }

        protected virtual object BuldOkResponseObject(IMetaInfo meta)
        {
            return new ResponseDto(meta);
        }

        protected virtual IMetaInfo BuildMetaInfo(HttpContext ctx)
        {
            return new MetaInfo
            {
                Uri = ctx.Request.GetAbsoluteUri(),
                MimeType = ctx.Request.ContentType ?? "application/octet-stream",
                Owner = GetOwner(ctx.User.Identity)
            };
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
    }
}