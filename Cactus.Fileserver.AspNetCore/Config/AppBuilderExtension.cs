using System;
using System.Net.Http;
using Cactus.Fileserver.AspNetCore.Middleware;
using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.AspNetCore.Config
{
    public static class AppBuilderExtension
    {
        public static IApplicationBuilder UseFileserver<T>(this IApplicationBuilder app,
            IFileserverConfig<HttpRequest, T> config) where T : class, IFileInfo, new()
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

        public static IApplicationBuilder MapDelFile<T>(this IApplicationBuilder app,
            IFileserverConfig<HttpRequest,T> config) where T : IFileInfo
        {
            // Branch requests to {path}/info or {path}?info - returns only file metadata
            app.MapWhen(c => HttpMethod.Delete.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase) &&
                             c.Request.Path.HasValue,
                builder =>
                {
                    builder.Run(async context =>
                    {
                        var handler = new DeleteFileHandler<T>(
                            builder.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory,
                            config.FileStorage());
                        await handler.Invoke(context);
                    });
                });
            return app;
        }

        public static IApplicationBuilder MapAddFile<T>(this IApplicationBuilder app,
            IFileserverConfig<HttpRequest, T> config) where T : class, IFileInfo, new()
        {
            app.MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder =>
                {
                    builder.Run(async context =>
                    {
                        var handler =
                            new AddFileHandler<T>(
                                builder.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory,
                                config.NewFilePipeline());
                        await handler.Invoke(context);
                    });
                });
            return app;
        }



        public static IApplicationBuilder MapGetFile<T>(this IApplicationBuilder app,
            IFileserverConfig<HttpRequest, T> config) where T : class, IFileInfo, new()
        {
            app.MapWhen(c => HttpMethod.Get.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder =>
                {
                    builder.Run(async context =>
                    {
                        var handler = new GetFileHandler<T>(
                            builder.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory,
                            config.GetFilePipeline());
                        await handler.Invoke(context);
                    });
                });
            return app;
        }
    }
}