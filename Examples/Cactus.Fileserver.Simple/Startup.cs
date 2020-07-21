using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Cactus.Fileserver.Aspnet.Config;
using Cactus.Fileserver.Aspnet.Middleware;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
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
            //url += url.EndsWith('/') ? "files/" : "/files/";     //<--- In case of using sub-path

            services
                .AddLogging();

            //var baseFolder = Path.GetTempPath();
            services.AddScoped<IUriResolver, AllInTheSameFolderUriResolver>(c => new AllInTheSameFolderUriResolver(new Uri(url), c.GetRequiredService<IWebHostEnvironment>().WebRootPath));
            services.AddScoped<IMetaInfoStorage, LocalMetaInfoStorage>(c => new LocalMetaInfoStorage(c.GetRequiredService<IWebHostEnvironment>().WebRootPath, c.GetRequiredService<ILogger<LocalMetaInfoStorage>>()));
            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddScoped<IStoredNameProvider, RandomNameProvider>();
            services.AddScoped<IFileStorageService, FileStorageService>();


            //.AddLocalFileserver(new Uri(url),
            //    c => c.GetRequiredService<IWebHostEnvironment>().WebRootPath,
            //    c => new PipelineBuilder()
            //        .UseMultipartContent()
            //        .ExtractFileinfo()
            //        .ReadContentStream()
            //        .ResizeIfLargeThen(c.GetRequiredService<IImageResizerService>(), 1000, 1000) //Force resizing for large images
            //        //.AcceptOnlyImageContent() //To accept only image content
            //        .Store(c.GetRequiredService<IFileStorageService>()))
            //.AddSingleton<IImageResizerService, ImageResizerService>()
            //.Configure<ResizingOptions>(o =>
            //{
            //    o.MandatoryInstructions = new ResizeInstructions { MaxWidth = 2000, MaxHeight = 2000 }; //Max size to operate width
            //    o.DefaultInstructions = new ResizeInstructions { KeepAspectRatio = true };
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();
            var storageFolder = new DirectoryInfo(env.WebRootPath);
            if (!storageFolder.Exists)
                storageFolder.Create();
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var x1 = scope.ServiceProvider.GetService<IUriResolver>();
                var x = scope.ServiceProvider.GetService<IFileStorageService>();
            }

            app
                .UseDeveloperExceptionPage()
                     //.Map("/files", branch => branch       //<--- In case of using sub-path
                     //.UseDynamicResizing()
                     .UseGetFile(b => b
                        .UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(storageFolder.FullName, ExclusionFilters.None),
                            DefaultContentType = "application/octet-stream",
                            ServeUnknownFileTypes = true, //<---- do not use on production to prevent upload executable content like viruses
                            ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string>
                            {
                                { ".json", "application/json"},
                                { ".svg", "image/svg+xml"},
                                { ".png", "image/png"}
                            })
                        }))
                        .MapWhen(c => HttpMethod.Post.Method.Equals(c.Request.Method, StringComparison.OrdinalIgnoreCase), b => b
                                  .UseMiddleware<AddFilesFromMultipartContentHandler>()
                                  .UseMiddleware<AddFileHandler>())
                     .UseDelFile()

                //)

                //Default handler for requests that are not targeted to the file server
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
