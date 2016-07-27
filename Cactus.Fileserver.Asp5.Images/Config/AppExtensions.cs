using System;
using Cactus.Fileserver.Asp5.Config;
using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Asp5.Images.Config
{
    public static class AppBuilderExtension
    {
        public const string InfoPathSegment = "/info";

        public static IApplicationBuilder UseImageFileserver(this IApplicationBuilder app, IImageServerConfig config)
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
                            var handler = new InfoRequestHandler(config.FileStorage());
                            await handler.Handle(context);
                        });
                    });

                // Handler to return file content
                builder.Run(async context =>
                {
                    var handler = new ImageDataHandler(config.FileStorage(), config.DefaultInstructions);
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
