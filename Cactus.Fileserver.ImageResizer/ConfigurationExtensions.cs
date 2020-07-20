using System.Linq;
using System.Text;
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
    }

}
