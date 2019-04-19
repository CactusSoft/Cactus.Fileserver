using System;
using System.Collections.Generic;
using System.IO;
using Cactus.Fileserver.Config;
using Cactus.Fileserver.Middleware;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
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
    }

}
