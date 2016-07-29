using Cactus.Fileserver.Owin.Config;
using Owin;

namespace Cactus.Fileserver.Owin.Images.Config
{
    public static class AppBuilderExtension
    {
        public static IAppBuilder UseImageFileserver(this IAppBuilder app, IImageServerConfig config)
        {
            app.Map(config.Path, builder =>
            {
                // Branch requests to {path}/info - returns info about a file 
                builder.MapFileinfo(config);

                // Handler to return file content
                builder.Run(async context =>
                {
                    var handler = new ImageDataHandler(config.FileStorage(), config.DefaultInstructions,config.MandatoryInstructions);
                    await handler.Handle(context);
                });
            });
            return app;
        }
    }
}
