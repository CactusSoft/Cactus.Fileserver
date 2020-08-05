using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.ImageResizer
{
    public class DynamicResizingMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<DynamicResizingMiddleware> _log;

        public DynamicResizingMiddleware(RequestDelegate next, ILogger<DynamicResizingMiddleware> logger)
        {
            _next = next;
            _log = logger;
        }

        public async Task InvokeAsync(HttpContext context, IImageResizerService resizer, IFileStorageService storage)
        {
            if (!context.Request.QueryString.HasValue)
            {
                _log.LogDebug("No query string, no dynamic resizing");
                await _next(context);
                return;
            }

            var instructions = new ResizeInstructions(context.Request.QueryString);
            if (instructions.Width == null && instructions.Height == null)
            {
                _log.LogDebug("No resizing instruction found in query string");
                await _next(context);
                return;
            }

            _log.LogDebug("It looks like resizing is requested. Looking for the file mete data first...");
            var request = context.Request;
            MetaInfo metaData;
            try
            {
                metaData = await storage.GetInfo<MetaInfo>(request.GetAbsoluteUri());
                if (metaData.Origin != null && !metaData.Uri.Equals(metaData.Origin))
                {
                    _log.LogDebug("{0} is not origin, getting the origin for transformation", metaData.Uri);
                    metaData = await storage.GetInfo<MetaInfo>(metaData.Origin);
                }
            }
            catch (FileNotFoundException)
            {
                _log.LogDebug("No metadata found for the requested file. Let the request pass its way");
                await _next(context);
                return;
            }

            _log.LogDebug("Metadata found, let's see if we can resize it");
            if (!metaData.MimeType.Equals("image/jpeg") &&
                !metaData.MimeType.Equals("image/jpg") &&
                !metaData.MimeType.Equals("image/png"))
            {
                _log.LogInformation("The file type is {0}, resizing is not supported", metaData.MimeType);
                await _next(context);
                return;
            }

            _log.LogDebug("Let's try to find already resized version");
            var sizeKey = instructions.BuildSizeKey();
            if (metaData.Extra.TryGetValue(sizeKey, out var redirectUri))
            {
                _log.LogDebug("{0} size found, do redirect", sizeKey);
                context.Response.Redirect(redirectUri, true);
                return;
            }

            try
            {
                _log.LogDebug("Do resizing");
                using (var tempFile = new MemoryStream())
                using (var original = await storage.Get(request.GetAbsoluteUri()))
                {
                    resizer.Resize(original, tempFile, instructions);
                    var newFileInfo = new MetaInfo(metaData) {Origin = metaData.Uri, Uri = metaData.Uri.GetFolder() };
                    tempFile.Position = 0;
                    await storage.Create(tempFile, newFileInfo);
                    metaData.Extra.Add(sizeKey, newFileInfo.Uri.ToString());
                    await storage.UpdateInfo(metaData);
                    context.Response.Redirect(newFileInfo.Uri.ToString(), true);
                    return;
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Exception during dynamic resizing, redirect to the origin {0}", ex);
                context.Response.Redirect(metaData.Uri.ToString(), false);
            }

            await _next(context);
        }
    }
}