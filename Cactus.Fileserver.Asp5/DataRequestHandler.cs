using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Asp5
{
    public class DataRequestHandler
    {
        protected readonly IFileStorageService StorageService;

        public DataRequestHandler(IFileStorageService storageService)
        {
            StorageService = storageService;
        }

        public async Task Handle(HttpContext context)
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
            }
        }

        protected virtual async Task HandleGet(HttpContext context)
        {
            var uri = context.Request.GetAbsoluteUri();
            var info = StorageService.GetInfo(uri);
            var stream = await StorageService.Get(uri);
            context.Response.ContentType = info.MimeType;
            await stream.CopyToAsync(context.Response.Body);
        }

        protected virtual async Task HandleDelete(HttpContext context)
        {
            var uri = context.Request.GetAbsoluteUri();
            await StorageService.Delete(uri);
            context.Response.StatusCode = 204; //HttpStatusCode.NoContent
        }

        protected virtual async Task HandlePost(HttpContext context)
        {
            var streamContent = new StreamContent(context.Request.Body);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);

            var provider = await streamContent.ReadAsMultipartAsync();
            var firstFileContent = provider.Contents.FirstOrDefault(c => !string.IsNullOrWhiteSpace(c.Headers.ContentDisposition.FileName));
            if (firstFileContent != null)
            {
                await HandleNewFileRequest(context, firstFileContent);
                return; //process only the first one
            }

            // No file content found, response BAD REQUEST
            context.Response.StatusCode = 400;
        }

        protected virtual async Task HandleNewFileRequest(HttpContext context, HttpContent newFileContent)
        {
            var fileName = newFileContent.Headers.ContentDisposition.FileName ?? "";
            using (var stream = await newFileContent.ReadAsStreamAsync())
            {
                var info = new IncomeFileInfo
                {
                    MimeType = newFileContent.Headers.ContentType.ToString(),
                    Name = fileName.Trim('"'),
                    Size = (int)stream.Length,
                    Owner = context.User.Identity.Name
                };

                var uri = await StorageService.Create(stream, info);
                context.Response.StatusCode = 201;
                context.Response.Headers.Add("Location", new[] { uri.ToString() });
            }
        }
    }
}

