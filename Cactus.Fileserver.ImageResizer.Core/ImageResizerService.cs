using System;
using System.IO;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using ImageSharp;
using ImageSharp.Processing;

namespace Cactus.Fileserver.ImageResizer.Core
{
    public class ImageResizerService
    {
        private readonly Instructions defaultInstructions;
        private readonly Instructions mandatoryInstructions;

        public ImageResizerService(Instructions defaultInstructions, Instructions mandatoryInstructions)
        {
            this.defaultInstructions = defaultInstructions;
            this.mandatoryInstructions = mandatoryInstructions;
        }


        /// <summary>
        /// Apply instructions to an image.
        /// A good point to extra configuration of Image Resizer
        /// </summary>
        /// <param name="inputStream">Input image stream</param>
        /// <param name="outputStream">Output stream to write the result</param>
        /// <param name="instructions">Instructions to apply</param>
        /// <returns>Result image as a stream. Caller have to care about the stream disposing.</returns>
        public virtual void ProcessImage(Stream inputStream, Stream outputStream, Instructions instructions)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("inputStream is nor readable");
            if (!outputStream.CanWrite)
                throw new ArgumentException("outputStream is nor writable");

            var image = new Image(inputStream);
            var imageRatio = image.PixelRatio;
            var resampler = new BicubicResampler();
            instructions.Join(defaultInstructions);
            instructions.Join(mandatoryInstructions, true);
            if (instructions.Width != null || instructions.Height != null || instructions["maxwidth"] != null || instructions["maxheight"] != null)
            {
                GetActualSize(instructions,imageRatio);
                image.Resize(new ResizeOptions
                {
                    Sampler = new BicubicResampler(),
                    Size = new Size(instructions.Width.Value, instructions.Height.Value),
                    Mode = instructions.Mode??ResizeMode.Max
                });
            }

            image.SaveAsJpeg(outputStream);
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

    }
}
