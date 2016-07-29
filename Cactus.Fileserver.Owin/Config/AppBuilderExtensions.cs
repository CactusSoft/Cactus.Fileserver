using System;
using System.Linq;
using Owin;

namespace Cactus.Fileserver.Owin.Config
{
    public static class AppBuilderExtensions
    {
        public const string InfoQueryKey = "info";
        public const string InfoPathSegment = "/info";
        public static IAppBuilder UseFileserver(this IAppBuilder app, IFileserverConfig config)
        {
            app.Map(config.Path, builder =>
            {
                MapFileinfo(builder, config);

                // Handler to return file content
                builder.Run(async context =>
                {
                    var handler = new DataRequestHandler(config.FileStorage());
                    await handler.Handle(context);
                });
            });
            return app;
        }

        public static IAppBuilder MapFileinfo(this IAppBuilder app, IFileserverConfig config)
        {
            // Branch requests to {path}/info or {path}?info - returns only file metadata
            app.MapWhen(c => (c.Request.Path.HasValue &&
                                 c.Request.Path.Value.EndsWith(InfoPathSegment, StringComparison.OrdinalIgnoreCase)) ||
                                 c.Request.Query.Any(e => e.Key.Equals(InfoQueryKey, StringComparison.OrdinalIgnoreCase)),
            infoBuilder =>
            {
                infoBuilder.Run(async context =>
                {
                    var handler = new InfoRequestHandler(config.FileStorage());
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
