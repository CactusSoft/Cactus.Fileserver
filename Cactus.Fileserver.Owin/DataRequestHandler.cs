using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Owin
{
    public class DataRequestHandler
    {
        protected readonly IFileStorageService StorageService;

        public DataRequestHandler(IFileStorageService storageService)
        {
            this.StorageService = storageService;
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
                var uri=await HandleNewFileRequest(context, firstFileContent);
                await context.Response.ResponseOk(new {Uri = uri});
                return; //process only the first one
            }

            // No file content found, response BAD REQUEST
            context.Response.StatusCode = 400;
        }

        protected virtual async Task<Uri> HandleNewFileRequest(IOwinContext context, HttpContent newFileContent)
        {
            using (var stream = await newFileContent.ReadAsStreamAsync())
            {
                var info = new IncomeFileInfo
                {
                    MimeType = newFileContent.Headers.ContentType.ToString(),
                    Name = GetOriginalFileName(newFileContent),
                    Size = (int)stream.Length,
                    Owner = GetOwner(context)
                };
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
    }
}

