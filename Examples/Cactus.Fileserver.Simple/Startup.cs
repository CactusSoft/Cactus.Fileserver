using System;
using Cactus.Fileserver.Core.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (string.IsNullOrEmpty(url))
            {
                url= "http://localhost:18047";
            }

            loggerFactory.AddLog4Net();
            app.UseDeveloperExceptionPage()
                //.UseMiddleware<DynamicResizeMiddleware<ExtendedMetaInfo>>()
                //.UseStaticFiles(new StaticFileOptions
                //{
                //    DefaultContentType = "application/octet-stream",
                //    ServeUnknownFileTypes = true
                //})
                .UseFileserver(new ServerConfig(env.WebRootPath, env.WebRootPath, new Uri(url)))
                .Run(async context =>
                {
                    if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("There's nothing here my little friend.");
                    }
                    else
                    {
                        // Strange request.
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("You do something wrong. What are you awaited of? Christmas mystery?");
                    }
                });
        }
    }
}
