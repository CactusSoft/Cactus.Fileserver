using System;
using Cactus.Fileserver.AspNetCore.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cactus.Fileserver.Simple
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage()
                .UseStaticFiles(new StaticFileOptions
                {
                    DefaultContentType = "application/octet-stream",
                    ServeUnknownFileTypes = true
                })
                .UseFileserver(new ServerConfig(env.WebRootPath, env.WebRootPath, new Uri("http://localhost:18047")))
                .Run(async context =>
                {
                    // Strange request, let's just say "BAD REQUEST"
                    // Instead, you could start your MVC handler here for instance  
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Default handler. There's nothing here my little friend.");
                });
        }
    }
}
