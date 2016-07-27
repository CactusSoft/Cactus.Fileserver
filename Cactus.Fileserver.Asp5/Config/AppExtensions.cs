using System;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Asp5.Config
{
    public static class AppBuilderExtension
    {
        public const string InfoQueryKey = "info";
        public const string InfoPathSegment = "/info";
        public static IApplicationBuilder UseFileserver(this IApplicationBuilder app, IFileserverConfig config)
        {
            app.Map(config.Path, builder =>
            {
                // Branch requests to {path}/info or {path}?info - returns only file metadata
                builder.MapWhen(c => (c.Request.Path.HasValue &&
                                     c.Request.Path.Value.EndsWith(InfoPathSegment, StringComparison.OrdinalIgnoreCase)) ||
                                     c.Request.Query.ContainsKey(InfoQueryKey),
                infoBuilder =>
                {
                    infoBuilder.Run(async context =>
                    {
                        var handler = new InfoRequestHandler(config.FileStorage());
                        await handler.Handle(context);
                    });
                });

                // Handler to return file content
                builder.Run(async context =>
                {
                    var handler = new DataRequestHandler(config.FileStorage());
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
