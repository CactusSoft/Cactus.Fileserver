using System;
using System.Threading.Tasks;
using Cactus.Fileserver.AspNetCore.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LocalFileserver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();
            var fileStorageFolder = env.WebRootPath;
            var metaStorageFolder = fileStorageFolder;
            var config = new ServerConfig(fileStorageFolder, metaStorageFolder, new Uri("http://localhost:38420"));

            app
                .UseDeveloperExceptionPage()
                .UseStaticFiles(new StaticFileOptions
                {
                    DefaultContentType = "application/octet-stream",
                    ServeUnknownFileTypes = true
                })
                .UseFileserver(config)
                .Run(context =>
                {
                    // Strange request, let's just say "BAD REQUEST"
                    // Instead, you could start your ASP.NET MVC handler here for instance  
                    context.Response.StatusCode = 400;
                    return Task.FromResult(0);
                });
        }
    }
}
