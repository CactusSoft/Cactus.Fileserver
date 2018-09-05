using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Logging;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.ImageResizer.Core
{
    public class DynamicResizeMiddleware
    {
        private readonly IFileStorageService _storage;
        private readonly ImageResizerService _resizer;
        private readonly RequestDelegate _next;
        private static readonly ILog Log = LogProvider.GetLogger(typeof(DynamicResizeMiddleware));

        public DynamicResizeMiddleware(RequestDelegate next, ImageResizerService resizer, IFileStorageService storage)
        {
            Log.Debug(".ctor");
            _next = next;
            _resizer = resizer;
            _storage = storage;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            MetaInfo metaData;
            try
            {
                metaData = _storage.GetInfo<MetaInfo>(request.GetAbsoluteUri());
                if (metaData.Origin != null && !metaData.Uri.Equals(metaData.Origin))
                {
                    Log.Debug("{0} is not origin, getting the origin for transformation", metaData.Uri);
                    metaData = _storage.GetInfo<MetaInfo>(metaData.Origin);
                }
            }
            catch (FileNotFoundException)
            {
                metaData = null;
            }

            if (metaData != null && request.QueryString.HasValue && metaData.MimeType.StartsWith("image") &&
                !metaData.MimeType.EndsWith("gif"))
            {
                var instructions = new Instructions(request.QueryString.Value);
                var sizeKey = instructions.GetSizeKey();
                if (metaData.Extra.TryGetValue(sizeKey, out var redirectUri))
                {
                    Log.Debug("{0} size found, do redirect", sizeKey);
                    context.Response.Redirect(redirectUri.ToString(), true);
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
            await _next(context);
        }
    }
}