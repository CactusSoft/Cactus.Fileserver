using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// Adds files from multipart content if the request is multipart. Otherwise, continue pipeline
    /// </summary>
    public class AddFilesFromMultipartContentHandler
    {
        protected static readonly string JsonMimeType = "application/json";
        private readonly RequestDelegate _next;
        private readonly ILogger<AddFilesFromMultipartContentHandler> _log;

        public AddFilesFromMultipartContentHandler(RequestDelegate next, ILogger<AddFilesFromMultipartContentHandler> log)
        {
            _next = next;
            _log = log;
        }

        public async Task InvokeAsync(HttpContext ctx, IFileStorageService fileStorage)
        {
            if (ctx.Request.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var content = new StreamContent(ctx.Request.Body);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(ctx.Request.ContentType);
                var provider = await content.ReadAsMultipartAsync();
                var resList = new List<object>(provider.Contents.Count);
                foreach (var contentPart in provider.Contents)
                {
                    try
                    {
                        var meta = await ProcessPart(ctx, contentPart, fileStorage);
                        resList.Add(BuldOkResponseObject(meta));
                    }
                    catch (Exception ex)
                    {
                        resList.Add(BuldErrorResponseObject(ex));
                    }
                }
                await ResponseForUpload(ctx, resList);
                _log.LogInformation("Served by {handler}", GetType().Name);
            }
            else
            {
                _log.LogInformation("Not a multipart, continue pipeline");
                await _next(ctx);
            }
        }

        protected virtual async Task ResponseForUpload(HttpContext ctx, ICollection<object> results)
        {
            if (results.Cast<ResponseDto>().Any(e => e.Error == null))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Created;
                ctx.Response.Headers.Add("Location", results.Cast<ResponseDto>().First(e => e.Error == null).Uri.ToString());
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            ctx.Response.ContentType = JsonMimeType;
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(results));
        }

        protected virtual async Task<IMetaInfo> ProcessPart(HttpContext ctx, HttpContent content,
            IFileStorageService fileStorage)
        {
            var meta = BuildMetaInfo(ctx, content.Headers);
            using (var stream = await content.ReadAsStreamAsync())
            {
                await fileStorage.Create(stream, meta);
            }
            return meta;
        }

        protected virtual object BuldOkResponseObject(IMetaInfo meta)
        {
            return new ResponseDto(meta);
        }

        protected virtual object BuldErrorResponseObject(Exception ex)
        {
            return new ResponseDto
            {
                Error = ex.Message
            };
        }

        protected virtual IMetaInfo BuildMetaInfo(HttpContext ctx, HttpContentHeaders contentHeaders)
        {
            return new MetaInfo
            {
                Uri = ctx.Request.GetAbsoluteUri(),
                MimeType = contentHeaders.ContentType?.ToString() ?? "application/octet-stream",
                OriginalName = contentHeaders.GetFileName() ?? "file",
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