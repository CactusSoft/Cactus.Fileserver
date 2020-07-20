using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Terminal handler for adding file
    /// </summary>
    public class AddFileHandler
    {
        protected static readonly string JsonMimeType = "application/json";
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<AddFileHandler> _log;

        public AddFileHandler(RequestDelegate next, IFileStorageService fileStorage, ILogger<AddFileHandler> log)
        {
            _fileStorage = fileStorage;
            _log = log;
            log.LogDebug(".ctor");
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            Validate(ctx);
            if (ctx.Request.HasFormContentType && ctx.Request.Form.Files.Count > 0)
            {
                _log.LogDebug("multiple file upload detected, store all of them");

                var multipartTasks = ctx.Request.Form.Files.Select(e => ProcessMultipartPiece(ctx, e)).ToList();
                await Task.WhenAll(multipartTasks);
                await ResponseForMultipleUpload(ctx, multipartTasks);
            }
            else
            {
                _log.LogDebug("Single content upload detected");
                var metaInfo = await ProcessSingleFileUpload(ctx);
                await ResponseForSingleUpload(ctx, metaInfo);
            }

            _log.LogInformation("Served by {handler}", GetType().Name);
        }

        private async Task ResponseForSingleUpload(HttpContext ctx, IMetaInfo metaInfo)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Created;
            ctx.Response.Headers.Add("Location", metaInfo.Uri.ToString());
            ctx.Response.ContentType = JsonMimeType;
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(BuldOkResponseObject(metaInfo)));
        }

        protected virtual async Task ResponseForMultipleUpload(HttpContext ctx, ICollection<Task<IMetaInfo>> uploadTasks)
        {
            if (uploadTasks.Any(e => !e.IsFaulted))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Created;
                ctx.Response.Headers.Add("Location", uploadTasks.First(e => !e.IsFaulted).Result.Uri.ToString());
            }
            else
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            ctx.Response.ContentType = JsonMimeType;
            var res = uploadTasks
                .Select(e => e.IsFaulted ? BuldErrorResponseObject(e.Exception) : BuldOkResponseObject(e.Result));
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(res));
        }

        protected virtual async Task<IMetaInfo> ProcessSingleFileUpload(HttpContext ctx)
        {
            var meta = new MetaInfo()
            {
                MimeType = ctx.Request.ContentType,
                Owner = GetOwner(ctx.User.Identity)
            };
            await _fileStorage.Create(ctx.Request.Body, meta);
            return meta;

        }

        protected virtual async Task<IMetaInfo> ProcessMultipartPiece(HttpContext ctx, IFormFile content)
        {
            var meta = new MetaInfo()
            {
                MimeType = content.ContentType,
                OriginalName = content.FileName,
                Owner = GetOwner(ctx.User.Identity)
            };

            using (var stream = content.OpenReadStream())
            {
                await _fileStorage.Create(stream, meta);
            }

            return meta;
        }

        protected virtual void Validate(HttpContext ctx)
        {
            _ = ctx.Request.Body ?? throw new ArgumentNullException(nameof(ctx) + '.' + nameof(ctx.Request) + '.' + nameof(ctx.Request.Body));
        }

        protected virtual object BuldOkResponseObject(IMetaInfo meta)
        {
            return new ResponseDto
            {
                Uri = meta.Uri,
                Extra = meta.Extra?.ToDictionary(e => e.Key, e => e.Value),
                Owner = meta.Owner,
                Origin = meta.Origin,
                OriginalName = meta.OriginalName,
                MimeType = meta.MimeType,
                Icon = meta.Icon
            };
        }

        protected virtual object BuldErrorResponseObject(Exception ex)
        {
            return new ResponseDto
            {
                Error = ex.Message
            };
        }

        /// <summary>
        ///     Try to extract original file name from the request
        /// </summary>
        /// <param name="contentHeaders"></param>
        /// <returns>Returns empty string if nothing found</returns>
        protected virtual string GetOriginalFileName(HttpContentHeaders contentHeaders)
        {
            return contentHeaders.ContentDisposition.FileName?.Trim('"');
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