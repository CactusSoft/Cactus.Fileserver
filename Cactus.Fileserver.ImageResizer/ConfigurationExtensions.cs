using System;
using System.IO;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cactus.Fileserver.ImageResizer
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddDynamicResizing(this IServiceCollection services, Instructions def = null, Instructions mandatory = null)
        {
            services.AddSingleton<IImageResizerService>(c => new ImageResizerService(
                def ?? new Instructions(""), mandatory ?? new Instructions()));
            return services;
        }

        public static IApplicationBuilder UseDynamicResizing(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DynamicResizingMiddleware>();
        }

        public static PipelineBuilder ApplyResizing(
            this PipelineBuilder builder, IImageResizerService resizer)
        {
            return builder.Use(next => async (request, content, stream, info) =>
            {
                //TODO: need to be refactored:
                // 1. Do not resize if the resizing is not required, i.e. original image size does not overdue maxheight/maxwidth params
                // 2. Pass mandatory & default params here instead of ImageResizerService constructor.
                // 3. Change the meaning of ImageResizerService constructor parameters. It should be a maximum operable size of image (in order to avoid OutOfMemory)
                if (info.MimeType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    using (var output = new MemoryStream())
                    {
                        resizer.ProcessImage(stream, output, new Instructions(request.QueryString.HasValue ? request.QueryString.Value : ""));
                        output.Position = 0;
                        return await next(request, content, output, info);
                    }
                }
                return await next(request, content, stream, info);
            });
        }
    }

}
