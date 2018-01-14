using System;
using System.IO;
using Cactus.Fileserver.ImageProcessing;
using Cactus.Fileserver.Owin.Config;
using Microsoft.Owin;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(Startup))]
namespace Cactus.Fileserver.ImageProcessing
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var fileStorageFolder = Path.GetTempPath();
            var metaStorageFolder = fileStorageFolder;
            app.UseFileserver(new ServerConfig(fileStorageFolder, metaStorageFolder, new Uri("http://localhost:19348/")))
                .UseStaticFiles(new StaticFileOptions
                {
                    DefaultContentType = "application/octet-stream",
                    ServeUnknownFileTypes = true
                })
                .Run(async context =>
                {
                    // Strange request, let's just say "BAD REQUEST"
                    // Instead, you could start your ASP.NET MVC handler here for instance  
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Default handler. There's nothing here my little friend.");
                });
        }
    }
}