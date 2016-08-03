using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using ImageResizer;
using Microsoft.Owin;
using Microsoft.Owin.Logging;

namespace Cactus.Fileserver.Owin.Images
{
    public class ImageDataHandler : DataRequestHandler
    {
        private readonly Instructions defaultInstructions;
        private readonly Instructions mandatoryInstructions;
        private readonly ILogger log;

        public ImageDataHandler(ILoggerFactory logFactory, IFileStorageService storageService, Instructions defaultInstructions, Instructions mandatoryInstructions) : base(logFactory, storageService)
        {
            this.defaultInstructions = defaultInstructions;
            this.mandatoryInstructions = mandatoryInstructions;
            log = logFactory.Create(typeof (ImageDataHandler).FullName);
        }

        protected override async Task<Uri> HandleNewFileRequest(IOwinContext context, HttpContent newFileContent)
        {
            if (newFileContent.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                log.WriteVerbose("Image content detected, start processing");
                var instructions = BuildInstructions(context.Request);
                using (var stream = await newFileContent.ReadAsStreamAsync())
                {
                    using (var streamToStore = ProcessImage(stream, instructions))
                    {
                        var info = BuildFileInfo(context, newFileContent);
                        return await StorageService.Create(streamToStore, info);
                    }
                }
            }

            log.WriteVerbose("No image content detected, run regular file storing workflow");
            return await base.HandleNewFileRequest(context, newFileContent);
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
        protected virtual Instructions BuildInstructions(IOwinRequest request)
        {
            Instructions res;
            if (request.QueryString.HasValue)
            {
                res = new Instructions(request.QueryString.Value);
                res.Join(defaultInstructions);
            }
            else
            {
                res = new Instructions(defaultInstructions);
            }

            // Override or add mandatory values
            res.Join(mandatoryInstructions, true);
            return res;
        }
    }
}

