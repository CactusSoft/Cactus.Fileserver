using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Config
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

        public static IApplicationBuilder UseAddFile(this IApplicationBuilder app)
        {
            app.MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase),
                builder => builder.UseMiddleware<AddFileHandler>());
            return app;
        }

        public static IApplicationBuilder UseGetFile(this IApplicationBuilder app, Action<IApplicationBuilder> configuration)
        {
            app.MapWhen(c => HttpMethod.Get.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase), configuration);
            return app;
        }
    }
}