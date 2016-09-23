using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
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
            log = logFactory.Create(typeof(ImageDataHandler).FullName);
        }

        protected override async Task<Uri> HandleNewFileRequest(IOwinContext context, HttpContent newFileContent)
        {
            if (newFileContent.Headers.ContentType.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                log.WriteVerbose("Image content detected, start processing");
                var instructions = BuildInstructions(context.Request);
                using (var stream = await newFileContent.ReadAsStreamAsync())
                {
                    using (var streamToStore = new MemoryStream())
                    {
                        var res = ProcessImage(stream, streamToStore, instructions);
                        streamToStore.Position = 0;
                        var info = BuildFileInfo(context, newFileContent, res);
                        return await StorageService.Create(streamToStore, info);
                    }
                }
            }

            log.WriteVerbose("No image content detected, run regular file storing workflow");
            return await base.HandleNewFileRequest(context, newFileContent);
        }

        /// <summary>
        /// Build file info based on input info and result of image conversion
        /// </summary>
        /// <param name="context"></param>
        /// <param name="newFileContent"></param>
        /// <param name="processingResult"></param>
        /// <returns></returns>
        private IFileInfo BuildFileInfo(IOwinContext context, HttpContent newFileContent, ImageProcessingResult processingResult)
        {
            var res = BuildFileInfo(context, newFileContent);
            if (processingResult.MediaType != null)
                res.MimeType = processingResult.MediaType;

            if (processingResult.FileExt != null && !res.OriginalName.EndsWith(processingResult.FileExt, StringComparison.OrdinalIgnoreCase))
            {
                // Need to correct file ext
                var dotIndex = res.OriginalName.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    res.OriginalName = res.OriginalName.Substring(0, dotIndex);
                }
                res.OriginalName += '.' + processingResult.FileExt;
            }
            return res;
        }

        /// <summary>
        /// Apply instructions to an image.
        /// A good point to extra configuration of Image Resizer
        /// </summary>
        /// <param name="inputStream">Input image stream</param>
        /// <param name="outputStream">Output stream to write the result</param>
        /// <param name="instructions">Instructions to apply</param>
        /// <returns>Result image as a stream. Caller have to care about the stream disposing.</returns>
        protected virtual ImageProcessingResult ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("inputStream is nor readable");
            if (!outputStream.CanWrite)
                throw new ArgumentException("outputStream is nor writable");

            var job = ImageBuilder.Current.Build(inputStream, outputStream, instructions);

            var res = new ImageProcessingResult();
            if (!string.IsNullOrEmpty(job.ResultFileExtension))
                res.FileExt = job.ResultFileExtension;
            if (!string.IsNullOrEmpty(job.ResultMimeType))
                res.MediaType = job.ResultMimeType;
            return res;
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

