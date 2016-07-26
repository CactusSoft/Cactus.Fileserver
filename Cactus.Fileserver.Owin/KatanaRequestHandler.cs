using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.Owin;

namespace Cactus.Fileserver.Owin
{
    public class KatanaRequestHandler
    {
        private readonly IFileStorageService storageService;

        public KatanaRequestHandler(IFileStorageService storageService)
        {
            this.storageService = storageService;
        }

        public async Task Handle(IOwinContext context)
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

        private async Task HandleGet(IOwinContext context)
        {
            var info = storageService.GetInfo(context.Request.Uri);
            var stream = await storageService.Get(context.Request.Uri);
            context.Response.ContentType = info.MimeType;
            await stream.CopyToAsync(context.Response.Body);
        }

        private async Task HandleDelete(IOwinContext context)
        {
            await storageService.Delete(context.Request.Uri);
        }

        private async Task HandlePost(IOwinContext context)
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
                        Owner = context.Authentication.User.Identity.Name
                    };
                    var uri = await storageService.Create(stream, info);
                    context.Response.StatusCode = 201;
                    context.Response.ReasonPhrase = "Created";
                    context.Response.Headers.Add("Location", new[] { uri.ToString() });
                    return; //process only the first one
                }
            }

            context.Response.StatusCode = 400;
            context.Response.ReasonPhrase = "Bad Request";
        }
    }
}

