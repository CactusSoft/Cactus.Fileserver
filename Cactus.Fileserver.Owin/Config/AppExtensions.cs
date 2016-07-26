using Owin;

namespace Cactus.Fileserver.Owin.Config
{
    public static class IAppBuilderExtensions
    {
        public static IAppBuilder UseFileserver(this IAppBuilder app, IFileserverConfig config)
        {
            app.Map(config.Path, builder =>
            {
                builder.Run(async context =>
                {
                    var handler = new KatanaRequestHandler(config.FileStorage());
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
