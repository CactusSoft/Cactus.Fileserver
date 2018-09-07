using System;
using Cactus.Fileserver.Config;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.Security;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
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

            //url += url.EndsWith('/') ? "files/" : "/files/";

            services.AddSingleton<ISecurityManager, NothingCheckSecurityManager>();
            services.AddSingleton<IMetaInfoStorage>(c => new LocalMetaInfoStorage(c.GetRequiredService<IHostingEnvironment>().WebRootPath));
            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddSingleton<IFileStorage>(c => new LocalFileStorage(new Uri(url), c.GetRequiredService<IStoredNameProvider>(), c.GetRequiredService<IHostingEnvironment>().WebRootPath));
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton<IImageResizerService>(c => new ImageResizerService(new Instructions(""), new Instructions("maxwidth=1440&maxheight=1440")));
            services.AddSingleton(c => new PipelineBuilder()
                                        .UseMultipartRequestParser()
                                        .UseOriginalFileinfo()
                                        .RunStoreFileAsIs(c.GetRequiredService<IFileStorageService>()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            app
                .UseDeveloperExceptionPage()
                //.Map("/files", branch => branch
                     .UseGetFile(b => b
                                 .UseMiddleware<DynamicResizeMiddleware>()
                                 .UseStaticFiles(new StaticFileOptions
                                 {
                                     FileProvider = new PhysicalFileProvider(env.WebRootPath, ExclusionFilters.None),
                                     DefaultContentType = "application/octet-stream",
                                     ServeUnknownFileTypes = true
                                 }))
                     .UseAddFile()
                     .UseDelFile()
                //)
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
