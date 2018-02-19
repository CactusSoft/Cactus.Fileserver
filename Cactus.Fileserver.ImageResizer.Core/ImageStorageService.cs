using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace Cactus.Fileserver.ImageResizer.Core
{
    public class ImageStorageService
    {
        private readonly IFileStorageService storageService;
        private readonly Instructions defaultInstructions;
        private readonly Instructions mandatoryInstructions;
        private readonly Instructions defaultThumbnailInstructions;
        private readonly Instructions mandatoryThumbnailInstructions;
        private readonly string paramsPrefix;
        protected readonly char IconDelimiter = '-';

        public ImageStorageService(IFileStorageService storageService, Instructions defaultInstructions, Instructions mandatoryInstructions, Instructions defaultThumbnailInstructions,
            Instructions mandatoryThumbnailInstructions,
            string paramsPrefix = "thmb-")
        {
            this.storageService = storageService;
            this.defaultInstructions = defaultInstructions;
            this.mandatoryInstructions = mandatoryInstructions;
            this.defaultThumbnailInstructions = defaultThumbnailInstructions;
            this.mandatoryThumbnailInstructions = mandatoryThumbnailInstructions;
            this.paramsPrefix = paramsPrefix;
        }

        /// <summary>
        /// Stores the image with applying instructions
        /// </summary>
        /// <param name="fileInfo">Income file info. Could be updated</param>
        /// <param name="file"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public virtual async Task<MetaInfo> StoreSingle(IFileInfo fileInfo, Stream file, string queryString)
        {
            var instructions = BuildInstructions(queryString);
            using (var streamToStore = new MemoryStream())
            {
                var res = ProcessImage(file, streamToStore, instructions);
                streamToStore.Position = 0;
                var storedFileInfo = BuildFileInfo(fileInfo, res);
                return await storageService.Create(streamToStore, storedFileInfo).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates image thumbnail & store both: thumbnail and original.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="file"></param>
        /// <param name="queryString">Query string will be used to get image & thubnail transformation parameters. The income params will be merged with default & mandatory</param>
        /// <returns>MetaInfo.Icon is thumbnail URI</returns>
        public virtual async Task<MetaInfo> StoreWithThumbnail(IFileInfo fileInfo, byte[] file, string queryString)
        {
            var thumbnailInstructions = BuildThumbnailInstructions(queryString);
            var originalInstructions = BuildInstructions(queryString);

            //Process thumbnail async
            var thumbnail = await ProcessImage(fileInfo, file, thumbnailInstructions).ConfigureAwait(false);
            fileInfo.Icon = thumbnail.Uri;
            var original = await ProcessImage(fileInfo, file, originalInstructions).ConfigureAwait(false);
            if (original.Extra == null)
                original.Extra = new Dictionary<string, string>();

            if (thumbnail.Extra != null)
            {
                foreach (var kvp in thumbnail.Extra.Select(e => new KeyValuePair<string, string>(nameof(MetaInfo.Icon) + IconDelimiter + e.Key, e.Value)))
                    original.Extra.Add(kvp);
            }

            var iconMimeTypeKey = nameof(MetaInfo.Icon) + IconDelimiter + nameof(MetaInfo.MimeType);
            if (!original.Extra.ContainsKey(iconMimeTypeKey))
                original.Extra.Add(iconMimeTypeKey, thumbnail.MimeType);

            return original;
        }

        /// <summary>
        /// Build file info object based on existing info & processing result. The result represents stored file but not the original one.
        /// For example if the original was a jpeg image, but after processing it's converted into png, file ext & mime type will be updated.
        /// </summary>
        /// <param name="fileInfo">Income file info</param>
        /// <param name="job">Processing result</param>
        /// <returns></returns>
        protected virtual IFileInfo BuildFileInfo(IFileInfo fileInfo, IImageFormat job)
        {
            var fileExt = job.Extension;
            var mediaType = job.MimeType ?? fileInfo.MimeType;

            var res = new IncomeFileInfo(fileInfo) { MimeType = mediaType };
            if (fileExt != null && !res.OriginalName.EndsWith(fileExt, StringComparison.OrdinalIgnoreCase))
            {
                // Need to correct file ext
                var dotIndex = res.OriginalName.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    res.OriginalName = res.OriginalName.Substring(0, dotIndex);
                }
                res.OriginalName += '.' + fileExt;
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
        public virtual IImageFormat ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("inputStream is nor readable");
            if (!outputStream.CanWrite)
                throw new ArgumentException("outputStream is nor writable");

            var image = new Image(inputStream);
            var imageRatio = image.PixelRatio;
            var resampler = new BicubicResampler();
            if (instructions.Width != null || instructions.Height != null || instructions["maxwidth"] != null || instructions["maxheight"] != null)
            {
                GetActualSize(instructions,imageRatio);
                image.Resize(instructions.Width.Value, instructions.Height.Value, resampler, false);
            }

            return image.SaveAsJpeg(outputStream).CurrentImageFormat;
        }

        protected virtual void  GetActualSize(Instructions instructions, double imageRatio)
        {
            double width = instructions.Width??-1;
            double height = instructions.Height??-1;
            var maxwidth = double.TryParse(instructions["maxwidth"], out var resW) ? resW : -1;
            var maxheight = double.TryParse(instructions["maxheight"], out var resH) ? resH : -1;

            //Eliminate cases where both a value and a max value are specified: use the smaller value for the width/height 
            if (maxwidth > 0 && width > 0) { width = Math.Min(maxwidth, width); maxwidth = -1; }
            if (maxheight > 0 && height > 0) { height = Math.Min(maxheight, height); maxheight = -1; }

            //Handle cases of width/maxheight and height/maxwidth as in legacy version 
            if (width != -1 && maxheight != -1) maxheight = Math.Min(maxheight, (width / imageRatio));
            if (height != -1 && maxwidth != -1) maxwidth = Math.Min(maxwidth, (height * imageRatio));


            //Move max values to width/height. FitMode should already reflect the mode we are using, and we've already resolved mixed modes above.
            width = Math.Max(width, maxwidth);
            height = Math.Max(height, maxheight);

            //Calculate missing value (a missing value is handled the same everywhere). 
            if (width > 0 && height <= 0) height = width / imageRatio;
            else if (height > 0 && width <= 0) width = height * imageRatio;

            instructions.Width = (int)Math.Round(width);
            instructions.Height = (int)Math.Round(height);
        }

        protected virtual async Task<MetaInfo> ProcessImage(IFileInfo fileInfo, byte[] rawData, Instructions instructions)
        {
            using (var stream = new MemoryStream(rawData))
            using (var streamToStore = new MemoryStream())
            {
                var res = ProcessImage(stream, streamToStore, instructions);
                var info = BuildFileInfo(fileInfo, res);
                streamToStore.Position = 0;
                return await storageService.Create(streamToStore, info);
            }
        }

        /// <summary>
        /// Builds image processing instructions based on income request & default settings.
        /// A good point to appply restrictions, always used parameters and so on.
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>Returns resizing settings that will be applied to.</returns>
        protected virtual Instructions BuildInstructions(string queryString)
        {
            Instructions res;
            if (queryString != null)
            {
                res = new Instructions(queryString);
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

        protected virtual Instructions BuildThumbnailInstructions(string queryString)
        {
            if (paramsPrefix == null || queryString == null)
            {
                return defaultThumbnailInstructions;
            }

            var thumbnailQueryParams = queryString.TrimStart('?')
                .Split('&')
                .Where(e => e.StartsWith(paramsPrefix, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Split('='))
                .Where(e => e.Length > 1)
                .Select(e => new KeyValuePair<string, string>(WebUtility.UrlDecode(e[0]), WebUtility.UrlDecode(e[1])))
                .Aggregate(new NameValueCollection(), (a, v) =>
                {
                    if (v.Value != null)
                    {
                        a.Add(v.Key.Substring(paramsPrefix.Length), v.Value);
                    }
                    return a;
                });

            Instructions res;
            if (thumbnailQueryParams.Count > 0)
            {
                res = new Instructions(thumbnailQueryParams);
                res.Join(defaultThumbnailInstructions);
            }
            else
            {
                res = new Instructions(defaultThumbnailInstructions);
            }

            res.Join(mandatoryThumbnailInstructions, true);
            return res;
        }
    }
}
