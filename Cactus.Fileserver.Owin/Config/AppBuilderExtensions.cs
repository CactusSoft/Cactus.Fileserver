using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Owin.Middleware;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace Cactus.Fileserver.Owin.Config
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseFileserver<T>(this IAppBuilder app, IFileserverConfig<IOwinRequest, T> config) where T : IFileInfo
        {
            if (!string.IsNullOrEmpty(config.Path) && config.Path != "/")
            {
                app
                    .Map(config.Path, builder =>
                    {
                        builder
                            .Use<DeleteFileMiddleware<T>>(builder.GetLoggerFactory(), config.FileStorage())
                            .Use<AddFileMiddleware>(builder.GetLoggerFactory(), config.NewFilePipeline());
                    });
            }
            else
            {
                app
                    .Use<DeleteFileMiddleware<T>>(app.GetLoggerFactory(), config.FileStorage())
                    .Use<AddFileMiddleware>(app.GetLoggerFactory(), config.NewFilePipeline());
            }

            return app;
        }
    }
}
