using System;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage.Config
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddLocalFileStorage(this IServiceCollection services, Action<LocalFileStorageOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<LocalFileStorageOptions>, LocalFileStorageOptionsValidator>());

            services.AddScoped<IUriResolver, BaseFolderUriResolver>();
            services.AddScoped<IFileStorage, LocalFileStorage>();
            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            return services;
        }

        public static IServiceCollection AddLocalMetaStorage(this IServiceCollection services, Action<LocalMetaStorageOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<LocalMetaStorageOptions>, LocalMetaStorageOptionsValidator>());
            
            services.AddScoped<IMetaInfoStorage, LocalMetaInfoStorage>();
            return services;
        }
    }

}
