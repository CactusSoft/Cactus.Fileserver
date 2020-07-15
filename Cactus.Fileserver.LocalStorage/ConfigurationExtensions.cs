using System;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.LocalStorage
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddLocalFileserver(this IServiceCollection services, Uri baseUri, Func<IServiceProvider, string> storageFolder, Func<IServiceProvider, FileProcessorDelegate> pipeline = null)
        {
            services.AddSingleton<IMetaInfoStorage>(c => new LocalMetaInfoStorage(storageFolder(c), c.GetRequiredService<ILogger<LocalMetaInfoStorage>>()));
            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddSingleton<IFileStorage>(c => new LocalFileStorage(baseUri, c.GetRequiredService<IStoredNameProvider>(), c.GetRequiredService<ILogger<LocalFileStorage>>(), storageFolder(c)));
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
