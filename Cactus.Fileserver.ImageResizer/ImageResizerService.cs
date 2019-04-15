using System;
using System.IO;
using Cactus.Fileserver.ImageResizer.Utils;
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
        private readonly Instructions _defaultInstructions;
        private readonly Instructions _mandatoryInstructions;

        public ImageResizerService(Instructions defaultInstructions, Instructions mandatoryInstructions)
        {
            _defaultInstructions = defaultInstructions;
            _mandatoryInstructions = mandatoryInstructions;
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

            instructions.Join(_defaultInstructions);
            instructions.Join(_mandatoryInstructions, true);

            if ((instructions.Width.HasValue || instructions.Height.HasValue) &&
                (instructions.MaxWidth.HasValue || instructions.MaxHeight.HasValue))
            {
                using (var image = Image.Load(inputStream, out var imageInfo))
                {
                    var targetSize = GetTargetSize(instructions, image.Width / (double)image.Height);
                    image.Mutate(x => x
                        .Resize(targetSize.Width, targetSize.Height));
                    image.Save(outputStream, imageInfo); // Automatic encoder selected based on extension.
                }
            }
        }

        internal static (int Width, int Height) GetTargetSize(Instructions instructions, double imageRatio)
        {
            if (instructions.MaxWidth == null && instructions.Width == null)
                throw new ArgumentException("Target width could not be determined");
            if (instructions.MaxHeight == null && instructions.Height == null)
                throw new ArgumentException("Target height could not be determined");

            double targetWidth = Math.Min(instructions.Width ?? 0, instructions.MaxWidth ?? 0);
            double targetHeight = Math.Min(instructions.Height ?? 0, instructions.MaxHeight ?? 0);


            if (instructions.KeepRatio)
            {
                if (imageRatio > 1)
                    targetHeight = targetWidth / imageRatio;
                else
                    targetWidth = targetHeight * imageRatio;
            }

            return ((int)Math.Round(targetWidth), (int)Math.Round(targetHeight));
        }
    }
}
