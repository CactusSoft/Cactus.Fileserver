using System;
using System.Net.Http;
using Cactus.Fileserver.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Config
{
    public static class AppBuilderExtension
    {
        public static IApplicationBuilder UseFileserver(this IApplicationBuilder app,
            IFileserverConfig config)
        {
            if (!string.IsNullOrEmpty(config.Path) && config.Path != "/")
            {
                app
                    .Map(config.Path, builder =>
                    {
                        builder
                            .MapDelFile(config)
                            .MapAddFile(config)
                            .MapGetFile(config);
                    });
            }
            else
            {
                app.MapDelFile(config);
                app.MapAddFile(config);
                app.MapGetFile(config);
            }

            return app;
        }

        public static IApplicationBuilder MapDelFile(this IApplicationBuilder app,
            IFileserverConfig config)
        {
            // Branch requests to {path}/info or {path}?info - returns only file metadata
            app.MapWhen(c => HttpMethod.Delete.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase) &&
                             c.Request.Path.HasValue,
                builder =>
                {
                    builder.Run(async context =>
                    {
                        var handler = new DeleteFileHandler(config.FileStorage());
                        await handler.Invoke(context);
                    });
                });
            return app;
        }

        public static IApplicationBuilder MapAddFile(this IApplicationBuilder app,
            IFileserverConfig config) 
        {
            app.MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder => builder.Run(async context =>
                {
                    var handler = new AddFileHandler(config.NewFilePipeline());
                    await handler.Invoke(context);
                }));
            return app;
        }
        
        public static IApplicationBuilder MapGetFile(this IApplicationBuilder app,
            IFileserverConfig config)
        {
            app.MapWhen(c => HttpMethod.Get.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder => config.GetPipeline(builder));
            return app;
        }
    }
}