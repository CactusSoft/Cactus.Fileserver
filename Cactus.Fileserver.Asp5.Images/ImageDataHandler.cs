using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using ImageResizer;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Asp5.Images
{
    public class ImageDataHandler : DataRequestHandler
    {
        private readonly Instructions defaultInstructions;

        public ImageDataHandler(IFileStorageService storageService, Instructions defaultInstructions) : base(storageService)
        {
            this.defaultInstructions = defaultInstructions;
        }

        protected override async Task HandleNewFileRequest(HttpContext context, HttpContent newFileContent)
        {
            if (newFileContent.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                var instructions = BuildInstructions(context.Request);
                using (var stream = await newFileContent.ReadAsStreamAsync())
                {
                    using (var streamToStore = ProcessImage(stream, instructions))
                    {
                        var info = new IncomeFileInfo
                        {
                            MimeType = newFileContent.Headers.ContentType.ToString(),
                            Name = (newFileContent.Headers.ContentDisposition.FileName ?? "").Trim('"'),
                            Size = (int)streamToStore.Length,
                            Owner = context.User.Identity.Name
                        };

                        var uri = await StorageService.Create(streamToStore, info);
                        context.Response.StatusCode = 201;
                        context.Response.Headers.Add("Location", new[] { uri.ToString() });
                    }
                }
            }
            else
            {
                await base.HandleNewFileRequest(context, newFileContent);
            }
        }

        /// <summary>
        /// Apply instructions to an image.
        /// A good point to extra configuration of Image Resizer
        /// </summary>
        /// <param name="inputStream">Input image stream</param>
        /// <param name="instructions">Instructions to apply</param>
        /// <returns>Result image as a stream. Caller have to care about the stream disposing.</returns>
        protected virtual Stream ProcessImage(Stream inputStream, Instructions instructions)
        {
            var outputStream = new MemoryStream();
            try
            {
                ImageBuilder.Current.Build(inputStream, outputStream, instructions);
                outputStream.Position = 0;
                return outputStream;
            }
            catch
            {
                outputStream.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Builds image processing instructions based on income request & default settings.
        /// A good point to appply restrictions, always used parameters and so on.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns resizing settings that will be applied to.</returns>
        protected virtual Instructions BuildInstructions(HttpRequest request)
        {
            return request.QueryString.HasValue ? new Instructions(request.QueryString.Value) : defaultInstructions;
        }
    }
}

