using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Owin.Middleware;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace Cactus.Fileserver.Owin.Config
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseFileserver(this IAppBuilder app, IFileserverConfig<IOwinRequest> config)
        {
            if (!string.IsNullOrEmpty(config.Path) && config.Path != "/")
            {
                app
                    .Map(config.Path, builder =>
                    {
                        builder
                            .Use<DeleteFileMiddleware>(builder.GetLoggerFactory(), config.FileStorage())
                            .Use<AddFileMiddleware>(builder.GetLoggerFactory(), config.NewFilePipeline());
                    });
            }
            else
            {
                app
                    .Use<DeleteFileMiddleware>(app.GetLoggerFactory(), config.FileStorage())
                    .Use<AddFileMiddleware>(app.GetLoggerFactory(), config.NewFilePipeline());
            }

            return app;
        }
    }
}
