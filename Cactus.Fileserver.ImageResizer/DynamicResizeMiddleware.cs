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
        private readonly IFileStorageService _storage;
        private readonly IImageResizerService _resizer;
        private readonly RequestDelegate _next;
        private readonly ILogger<DynamicResizingMiddleware> _log;

        public DynamicResizingMiddleware(RequestDelegate next, IImageResizerService resizer, IFileStorageService storage, ILogger<DynamicResizingMiddleware> logger)
        {
            _next = next;
            _resizer = resizer;
            _storage = storage;
            _log = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.QueryString.HasValue)
            {
                _log.LogDebug("No query string, no dynamic resizing");
                await _next(context);
                return;
            }

            if (!context.Request.Query.ContainsKey("height") && !context.Request.Query.ContainsKey("width"))
            {
                _log.LogDebug("No resizing instructions found, no dynamic resizing");
                await _next(context);
                return;
            }

            try
            {
                var request = context.Request;
                MetaInfo metaData;
                try
                {
                    metaData = _storage.GetInfo<MetaInfo>(request.GetAbsoluteUri());
                    if (metaData.Origin != null && !metaData.Uri.Equals(metaData.Origin))
                    {
                        _log.LogDebug("{0} is not origin, getting the origin for transformation", metaData.Uri);
                        metaData = _storage.GetInfo<MetaInfo>(metaData.Origin);
                    }
                }
                catch (FileNotFoundException)
                {
                    metaData = null;
                }

                if (metaData != null && request.QueryString.HasValue && metaData.MimeType.StartsWith("image") &&
                    !metaData.MimeType.Contains("gif") &&
                    !metaData.MimeType.Contains("svg"))
                {
                    var instructions = new Instructions(request.QueryString.Value);
                    var sizeKey = instructions.GetSizeKey();
                    if (metaData.Extra.TryGetValue(sizeKey, out var redirectUri))
                    {
                        _log.LogDebug("{0} size found, do redirect", sizeKey);
                        context.Response.Redirect(redirectUri, true);
                        return;
                    }

                    using (var tempFile = new MemoryStream())
                    using (var original = await _storage.Get(request.GetAbsoluteUri()))
                    {
                        _resizer.ProcessImage(original, tempFile, instructions);
                        var newFileInfo = new IncomeFileInfo(metaData);
                        tempFile.Position = 0;
                        var result = await _storage.Create(tempFile, newFileInfo);
                        Uri savedRedirectUri = null;
                        try
                        {
                            savedRedirectUri = _storage.GetRedirectUri(result.Uri);
                        }
                        catch (NotImplementedException)
                        {
                            savedRedirectUri = result.Uri;
                        }
                        finally
                        {
                            metaData.Extra.Add(sizeKey, savedRedirectUri.ToString());
                            await _storage.UpdateMetadata(metaData);
                            context.Response.Redirect(savedRedirectUri.ToString(), true);
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError("Exception during dynamic resizing, skip middleware and continue with regular pipeline: {0}", ex);
            }

            await _next(context);
        }
    }
}