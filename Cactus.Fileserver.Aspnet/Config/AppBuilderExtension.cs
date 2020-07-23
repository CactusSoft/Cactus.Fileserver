using System;
using System.Net.Http;
using Cactus.Fileserver.Aspnet.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Aspnet.Config
{
    public static class AppBuilderExtension
    {
        public static IApplicationBuilder UseDelFile(this IApplicationBuilder app)
        {
            app.MapWhen(c => HttpMethod.Delete.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase) &&
                             c.Request.Path.HasValue,
                builder => builder.UseMiddleware<DeleteFileHandler>());
            return app;
        }

        public static IApplicationBuilder UseDelFile<T>(this IApplicationBuilder app)
        {
            app.MapWhen(c => HttpMethod.Delete.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase) &&
                             c.Request.Path.HasValue,
                builder => builder.UseMiddleware<T>());
            return app;
        }

        public static IApplicationBuilder UseAddFile(this IApplicationBuilder app)
        {
            app.MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder => builder
                    .UseMiddleware<AddFilesFromMultipartContentHandler>()
                    .UseMiddleware<AddFileHandler>());
            return app;
        }

        public static IApplicationBuilder UseAddFile<T>(this IApplicationBuilder app)
        {
            app.MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder => builder.UseMiddleware<T>());
            return app;
        }

        public static IApplicationBuilder UseGetFile(this IApplicationBuilder app, Action<IApplicationBuilder> configuration)
        {
            app.MapWhen(c => HttpMethod.Get.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase), configuration);
            return app;
        }
    }
}