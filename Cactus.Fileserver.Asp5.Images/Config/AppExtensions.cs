using Microsoft.AspNetCore.Builder;

namespace Cactus.Fileserver.Asp5.Images.Config
{
    public static class AppBuilderExtension
    {
        public static IApplicationBuilder UseImageFileserver(this IApplicationBuilder app, IImageServerConfig config)
        {
            app.Map(config.Path, builder =>
            {
                // Branch requests to {path}/info - returns info about a file 
                Asp5.Config.AppBuilderExtension.MapFileinfo(builder, config);

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
