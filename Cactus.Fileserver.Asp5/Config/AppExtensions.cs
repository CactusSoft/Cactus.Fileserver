using System;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Asp5.Config
{
    public static class AppBuilderExtension
    {
        public const string InfoPathSegment = "/info";
        public static IApplicationBuilder UseFileserver(this IApplicationBuilder app, IFileserverConfig config)
        {
            app.Map(config.Path, builder =>
            {
                // Branch requests to {path}/info - returns info about a file 
                builder.MapWhen(c => c.Request.Path.HasValue &&
                                     c.Request.Path.Value.EndsWith(InfoPathSegment, StringComparison.OrdinalIgnoreCase),
                infoBuilder =>
                {
                    infoBuilder.Run(async context =>
                    {
                        var handler = new Asp5InfoRequestHandler(config.FileStorage());
                        await handler.Handle(context);
                    });
                });

                // Handler to return file content
                builder.Run(async context =>
                {
                    var handler = new Asp5DataRequestHandler(config.FileStorage());
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
