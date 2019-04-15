using System;
using System.IO;
using Cactus.Fileserver.ImageResizer.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;


namespace Cactus.Fileserver.ImageResizer
{
    public interface IImageResizerService
    {
        /// <summary>
        /// Apply instructions to an image.
        /// A good point to extra configuration of Image Resizer
        /// </summary>
        /// <param name="inputStream">Input image stream</param>
        /// <param name="outputStream">Output stream to write the result</param>
        /// <param name="instructions">Instructions to apply</param>
        /// <returns>Result image as a stream. Caller have to care about the stream disposing.</returns>
        void ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions);
    }

    public class ImageResizerService : IImageResizerService
    {
        private readonly IOptionsMonitor<ResizingOptions> _optionsMonitor;
        private readonly ILogger<ImageResizerService> _log;

        public ImageResizerService(IOptionsMonitor<ResizingOptions> optionsMonitor, ILogger<ImageResizerService> log)
        {
            _optionsMonitor = optionsMonitor;
            _log = log;
        }

        //public virtual void ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions)
        //{
        //    if (!inputStream.CanRead)
        //        throw new ArgumentException("inputStream is nor readable");
        //    if (!outputStream.CanWrite)
        //        throw new ArgumentException("outputStream is nor writable");

        //    var image = new Image(inputStream); //<<<< DISPOSABLE!!!
        //    var imageRatio = image.PixelRatio;
        //    instructions.Join(_defaultInstructions);
        //    instructions.Join(_mandatoryInstructions, true);
        //    if (instructions.Width != null || instructions.Height != null || instructions["maxwidth"] != null || instructions["maxheight"] != null)
        //    {
        //        GetActualSize(instructions,imageRatio);
        //        image.Resize(new ResizeOptions
        //        {
        //            Sampler = new BicubicResampler(),
        //            Size = new Size(instructions.Width.Value, instructions.Height.Value),
        //            Mode = instructions.Mode??ResizeMode.Max
        //        });
        //    }

        //    image.SaveAsJpeg(outputStream);
        //}

        public virtual void ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("inputStream is nor readable");
            if (!outputStream.CanWrite)
                throw new ArgumentException("outputStream is nor writable");
            var options = _optionsMonitor.CurrentValue;
            instructions.Join(options.DefaultInstructions);
            instructions.Join(options.MandatoryInstructions, true);

            if (!(instructions.Width.HasValue || instructions.MaxWidth.HasValue) ||
                !(instructions.Height.HasValue || instructions.MaxHeight.HasValue))
                throw new InvalidOperationException("Resizing instructions are not complete, unable to resize");

            _log.LogDebug("Start resizing...");
            using (var image = Image.Load(inputStream, out var imageInfo))
            {
                var targetSize = GetTargetSize(instructions, image.Width / (double)image.Height);
                _log.LogDebug("Format {0}, original size {1}x{2}:{3} bytes, target size {4}x{5}", imageInfo.Name, image.Width, image.Height, inputStream.Length, targetSize.Width, targetSize.Height);
                image.Mutate(x => x.Resize(targetSize.Width, targetSize.Height));
                image.Save(outputStream, imageInfo); // Automatic encoder selected based on extension.
                _log.LogDebug("Resizing complete, output image size: {0}x{1}:{2} bytes", targetSize.Width, targetSize.Height, outputStream.Length);
            }
        }

        internal static (int Width, int Height) GetTargetSize(Instructions instructions, double imageRatio)
        {
            if (instructions.MaxWidth == null && instructions.Width == null)
                throw new ArgumentException("Target width could not be determined");
            if (instructions.MaxHeight == null && instructions.Height == null)
                throw new ArgumentException("Target height could not be determined");

            var targetWidth = Math.Min(instructions.Width ?? int.MaxValue, instructions.MaxWidth ?? 0);
            var targetHeight = Math.Min(instructions.Height ?? int.MaxValue, instructions.MaxHeight ?? 0);

            if (targetHeight == 0 || targetHeight == int.MaxValue || targetWidth == 0 || targetWidth == int.MaxValue)
                throw new ArgumentException("Invalid combination of height/with & maxheight/maxwidth");

            if (instructions.KeepRatio)
            {
                var mayUseWidthBase = instructions.Width.HasValue && instructions.Width.Value <= targetWidth;
                var mayUseHeightBase = instructions.Height.HasValue && instructions.Height.Value <= targetHeight;

                if (mayUseWidthBase && mayUseHeightBase)
                {
                    if (imageRatio > 1)
                        targetHeight = (int)Math.Round(targetWidth / imageRatio);
                    else
                        targetWidth = (int)Math.Round(targetHeight * imageRatio);
                }else if (mayUseHeightBase)
                {
                    targetWidth = (int)Math.Round(targetHeight * imageRatio);
                }else if (mayUseWidthBase)
                {
                    targetHeight = (int)Math.Round(targetWidth / imageRatio);
                }
                else
                {
                    if (imageRatio > 1)
                        targetHeight = (int)Math.Round(targetWidth / imageRatio);
                    else
                        targetWidth = (int)Math.Round(targetHeight * imageRatio);
                }
            }
            return (targetWidth, targetHeight);
        }
    }

    public class ResizingOptions
    {
        public Instructions DefaultInstructions { get; set; }
        public Instructions MandatoryInstructions { get; set; }
    }
}
