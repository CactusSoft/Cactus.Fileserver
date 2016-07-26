using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Asp5
{
    public class Asp5DataRequestHandler
    {
        private readonly IFileStorageService storageService;

        public Asp5DataRequestHandler(IFileStorageService storageService)
        {
            this.storageService = storageService;
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

        private async Task HandleGet(HttpContext context)
        {
            var uri = context.Request.GetAbsoluteUri();
            var info = storageService.GetInfo(uri);
            var stream = await storageService.Get(uri);
            context.Response.ContentType = info.MimeType;
            await stream.CopyToAsync(context.Response.Body);
        }

        private async Task HandleDelete(HttpContext context)
        {
            var uri = context.Request.GetAbsoluteUri();
            await storageService.Delete(uri);
            context.Response.StatusCode = 204; //HttpStatusCode.NoContent
        }

        private async Task HandlePost(HttpContext context)
        {
            var streamContent = new StreamContent(context.Request.Body);
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);

            var provider = await streamContent.ReadAsMultipartAsync();
            foreach (var httpContent in provider.Contents)
            {
                var fileName = httpContent.Headers.ContentDisposition.FileName;
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

                using (var stream = await httpContent.ReadAsStreamAsync())
                {
                    var info = new IncomeFileInfo
                    {
                        MimeType = httpContent.Headers.ContentType.ToString(),
                        Name = fileName.Trim('"'),
                        Size = (int)stream.Length,
                        Owner = context.User.Identity.Name
                    };
                    var uri = await storageService.Create(stream, info);
                    context.Response.StatusCode = 201;
                    context.Response.Headers.Add("Location", new[] { uri.ToString() });
                    return; //process only the first one
                }
            }

            context.Response.StatusCode = 400;
        }
    }
}

