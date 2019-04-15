using System;
using System.IO;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Security;
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
            var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (string.IsNullOrEmpty(url))
            {
                url = "http://localhost:18047";
            }
            //url += url.EndsWith('/') ? "files/" : "/files/";     //<--- In case of using sub-path

            services
                .AddLogging()
                .AddSingleton<ISecurityManager, NothingCheckSecurityManager>()
                .AddLocalFileserver(new Uri(url),
                    c => c.GetRequiredService<IHostingEnvironment>().WebRootPath,
                    c => new PipelineBuilder()
                        .UseMultipartContent()
                        .ExtractFileinfo()
                        .ReadContentStream()
                        //.AcceptOnlyImageContent() //To accept only image content
                        //.ApplyResizing(c.GetRequiredService<IImageResizerService>()) // <-- BE CAREFUL, IT DOES RESIZING ALL THE TIME
                        .Store(c.GetRequiredService<IFileStorageService>()))
                .AddSingleton<IImageResizerService, ImageResizerService>()
                .Configure<ResizingOptions>(o =>
                {
                    o.MandatoryInstructions = new Instructions("maxwidth=1000&maxheight=1000");
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            app
                .UseDeveloperExceptionPage()
                     //.Map("/files", branch => branch       //<--- In case of using sub-path
                     .UseDynamicResizing()
                     .UseLocalFileserver(new DirectoryInfo(env.WebRootPath))
                //)
                .Run(async context =>
                {
                    if (context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("There's nothing here, my little friend.");
                    }
                    else
                    {
                        // Strange request.
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("You do something wrong. What are you waiting for, a Christmas mystery?");
                    }
                });
        }
    }
}
