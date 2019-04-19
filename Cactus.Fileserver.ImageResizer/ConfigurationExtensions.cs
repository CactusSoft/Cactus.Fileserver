using System;
using System.IO;
using System.Linq;
using System.Text;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cactus.Fileserver.ImageResizer
{
    public static class ConfigurationExtensions
    {
        private static readonly byte[][] ImageHeaders = new[]
        {
            Encoding.ASCII.GetBytes("BM"), // BMP
            Encoding.ASCII.GetBytes("GIF"), // GIF
            new byte[]{ 137, 80, 78, 71 }, // PNG
            new byte[]{ 73, 73, 42 }, // TIFF
            new byte[]{ 77, 77, 42 }, // TIFF
            new byte[]{ 255, 216, 255, 224 }, // jpeg
            new byte[]{ 255, 216, 255, 225 } // jpeg canon
        };

        private static readonly int MaxHeaderLength = ImageHeaders.Max(e => e.Length);
        public static readonly string NotAnImageExceptionMessage = "Income stream doesn't look to be an image";


        public static IServiceCollection AddDynamicResizing(this IServiceCollection services)
        {
            services.AddSingleton<IImageResizerService, ImageResizerService>();
            return services;
        }

        public static IApplicationBuilder UseDynamicResizing(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DynamicResizingMiddleware>();
        }

        public static PipelineBuilder ResizeIfLargeThen(
            this PipelineBuilder builder, IImageResizerService resizer, int maxWidth, int maxHeight)
        {
            var instructions = new ResizeInstructions { Width = maxWidth, Height = maxHeight, KeepAspectRatio = true };
            return builder.Use(next => async (request, content, stream, info) =>
            {
                if (!stream.CanSeek)
                {
                    //Log warn that the income stream is not seekable
                    return await next(request, content, stream, info);
                }

                if (!info.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) &&
                    !info.MimeType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) &&
                    !info.MimeType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) &&
                    !info.MimeType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
                {
                    //Log debug that the mimetype is not resizable image
                    return await next(request, content, stream, info);
                }

                var probe = resizer.Probe(stream);
                stream.Position = 0;
                if (probe.Width > instructions.Width || probe.Height > instructions.Height)
                {
                    using (var output = new MemoryStream())
                    {
                        resizer.Resize(stream, output, instructions);
                        output.Position = 0;
                        return await next(request, content, output, info);
                    }
                }

                return await next(request, content, stream, info);
            });
        }

        public static PipelineBuilder AcceptOnlyImageContent(this PipelineBuilder builder)
        {
            return builder.Use(next => async (request, content, stream, info) =>
            {
                if (stream != null && stream.CanSeek)
                {
                    var head = new byte[MaxHeaderLength];
                    stream.Read(head, 0, head.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (!ImageHeaders.Any(x => x.SequenceEqual(head.Take(x.Length))))
                    {
                        throw new ArgumentException(NotAnImageExceptionMessage);
                    }
                }
                return await next(request, content, stream, info);
            });
        }
    }

}
