using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cactus.Fileserver.ImageResizer
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddDynamicResizing(this IServiceCollection services,
            Action<ResizingOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.AddSingleton<IImageResizerService, ImageResizerService>();
            services.AddSingleton<IUriResolver, DefaultUriResolver>();
            return services;
        }

        public static IServiceCollection AddDynamicResizing<T>(
            this IServiceCollection services,
            T uriResolver,
            Action<ResizingOptions> configureOptions) where T : class, IUriResolver
        {
            services.Configure(configureOptions);
            services.AddSingleton<IImageResizerService, ImageResizerService>();
            services.AddSingleton<IUriResolver, T>();
            return services;
        }

        public static IApplicationBuilder UseDynamicResizing(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DynamicResizingMiddleware>();
        }
    }
}