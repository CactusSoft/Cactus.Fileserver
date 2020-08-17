using System;
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
                _log.LogDebug("No resizing instruction found in query string, continue pipeline");
                await _next(context);
                return;
            }

            _log.LogDebug("It looks like resizing is requested. Looking for the file mete data first...");
            var request = context.Request;
            MetaInfo metaData;
            try
            {
                metaData = await storage.GetInfo<MetaInfo>(request.GetAbsoluteUri());
                _ = metaData ?? throw new FileNotFoundException($"No metadata found for {request.GetAbsoluteUri()}");

                if (metaData.Origin != null && !metaData.Uri.Equals(metaData.Origin))
                {
                    _log.LogDebug("{uri} is not origin, getting the origin for transformation", metaData.Uri);
                    metaData = await storage.GetInfo<MetaInfo>(metaData.Origin);
                }
            }
            catch (FileNotFoundException)
            {
                _log.LogWarning("No metadata found for the requested file, continue pipeline");
                await _next(context);
                return;
            }

            _log.LogDebug("Metadata found, let's see if we can resize it");
            if (!metaData.MimeType.Equals("image/jpeg") &&
                !metaData.MimeType.Equals("image/jpg") &&
                !metaData.MimeType.Equals("image/png"))
            {
                _log.LogInformation("The file type is {content-type}, resizing is not supported, continue pipeline", metaData.MimeType);
                await _next(context);
                return;
            }

            _log.LogDebug("Let's try to find already resized version");
            var sizeKey = instructions.BuildSizeKey();
            if (metaData.Extra.TryGetValue(sizeKey, out var redirectUri))
            {
                _log.LogDebug("{size_key} size found, do redirect", sizeKey);
                context.Response.Redirect(redirectUri, true);
                _log.LogInformation("Served by {handler}", GetType().Name);
                return;
            }

            try
            {
                _log.LogDebug("Do resizing");
                using (var tempFile = new MemoryStream())
                using (var original = await storage.Get(request.GetAbsoluteUri()))
                {
                    resizer.Resize(original, tempFile, instructions);
                    var newFileInfo = new MetaInfo(metaData) { Origin = metaData.Uri, Uri = metaData.Uri.GetFolder() };
                    tempFile.Position = 0;
                    await storage.Create(tempFile, newFileInfo);
                    metaData.Extra.Add(sizeKey, newFileInfo.Uri.ToString());
                    await storage.UpdateInfo(metaData);
                    _log.LogDebug("Resized successfully to {size_key}, redirect to {url}", sizeKey, newFileInfo.Uri);
                    context.Response.Redirect(newFileInfo.Uri.ToString(), true);
                    _log.LogInformation("Served by {handler}", GetType().Name);
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