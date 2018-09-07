using System;
using Cactus.Fileserver.Config;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;

namespace Cactus.Fileserver.LocalStorage
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddLocalFileserver(this IServiceCollection services, Uri baseUri, Func<IServiceProvider, string> storageFolder, Func<IServiceProvider, FileProcessorDelegate> pipeline = null)
        {
            services.AddSingleton<IMetaInfoStorage>(c => new LocalMetaInfoStorage(storageFolder(c)));
            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddSingleton<IFileStorage>(c => new LocalFileStorage(baseUri, c.GetRequiredService<IStoredNameProvider>(), storageFolder(c)));
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton(c => pipeline != null ? pipeline(c) :
                new PipelineBuilder()
                .UseMultipartContent()
                .ExtractFileinfo()
                .Store(c.GetRequiredService<IFileStorageService>()));
            return services;
        }

        public static IApplicationBuilder UseLocalFileserver(this IApplicationBuilder app, string storageFolder)
        {
            return app
                .UseGetFile(b => b

                    .UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(storageFolder, ExclusionFilters.None),
                        DefaultContentType = "application/octet-stream",
                        ServeUnknownFileTypes = true
                    }))
                .UseAddFile()
                .UseDelFile();
        }
    }

}
