using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace Cactus.Fileserver.Owin
{
    public class DataRequestHandler
    {
        protected readonly IFileStorageService StorageService;
        private readonly ILogger log;

        public DataRequestHandler(ILoggerFactory logFactory, IFileStorageService storageService)
        {
            log = logFactory.Create(typeof(DataRequestHandler).FullName);
            StorageService = storageService;
        }

        public virtual async Task Handle(IOwinContext context)
        {
            if (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await HandlePost(context);
            }
            else if (context.Request.Method.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
            {
                await HandleDelete(context);
            }
            else if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await HandleGet(context);
            }
            else
            {
                log.WriteWarning("Method not allowed: {0}", context.Request.Method);
                context.Response.StatusCode = 405;
                context.Response.ReasonPhrase = "Method not allowed";
            }
        }

        protected virtual async Task HandleGet(IOwinContext context)
        {
            var info = StorageService.GetInfo(context.Request.Uri);
            var stream = await StorageService.Get(context.Request.Uri);
            context.Response.ContentType = info.MimeType;
            await stream.CopyToAsync(context.Response.Body);
        }

        protected virtual async Task HandleDelete(IOwinContext context)
        {
            await StorageService.Delete(context.Request.Uri);
        }

        protected virtual async Task HandlePost(IOwinContext context)
        {
            var streamContent = new StreamContent(context.Request.Body);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);

            var provider = await streamContent.ReadAsMultipartAsync();
            var firstFileContent = provider.Contents.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Headers.ContentDisposition.FileName));
            if (firstFileContent != null)
            {
                var uri = await HandleNewFileRequest(context, firstFileContent);
                await context.Response.ResponseOk(new { Uri = uri });
                return; //process only the first one
            }

            log.WriteWarning("No file content found, response 400 BAD REQUEST");
            context.Response.StatusCode = 400;
        }

        protected virtual async Task<Uri> HandleNewFileRequest(IOwinContext context, HttpContent newFileContent)
        {
            using (var stream = await newFileContent.ReadAsStreamAsync())
            {
                var info = BuildFileInfo(context, newFileContent);
                return await StorageService.Create(stream, info);
            }
        }

        /// <summary>
        /// Try to extract original file name from the request
        /// </summary>
        /// <param name="httpContent"></param>
        /// <returns>Returns empty string if nothing found</returns>
        protected virtual string GetOriginalFileName(HttpContent httpContent)
        {
            return httpContent.Headers.ContentDisposition.FileName?.Trim('"') ?? "";
        }

        /// <summary>
        /// Returns a string that represent file owner based on authentication context.
        /// By default returns Identity.Name or nul if user is not authenticated.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Owner as a string or null</returns>
        protected virtual string GetOwner(IOwinContext context)
        {
            return context.Authentication?.User?.Identity?.Name;
        }

        /// <summary>
        /// Returns file info extracted from the request.
        /// Good point to add extra fields or override some of them
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual IFileInfo BuildFileInfo(IOwinContext context, HttpContent content)
        {
            return new IncomeFileInfo
            {
                MimeType = content.Headers.ContentType.ToString(),
                Name = GetOriginalFileName(content),
                Owner = GetOwner(context)
            };
        }
    }
}

