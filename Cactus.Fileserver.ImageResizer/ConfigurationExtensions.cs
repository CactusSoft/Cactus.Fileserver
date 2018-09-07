using Cactus.Fileserver.Config;
using Cactus.Fileserver.ImageResizer.Utils;
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
            return app.UseMiddleware<DynamicResizeMiddleware>();
        }
    }

}
