using System;
using Amazon.Runtime;
using Amazon.S3;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileserverBuilder
    {

    }
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddS3FileStorage(this IServiceCollection services, Action<S3FileStorageOptions> configureOptions)
        {
            services.Configure(configureOptions);

            services.AddScoped<IAmazonS3, AmazonS3Client>(c =>
             {
                 var config = c.GetRequiredService<IOptions<S3FileStorageOptions>>().Value;
                 return new AmazonS3Client(new BasicAWSCredentials(config.AccessKey, config.SecretKey));
             });

            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddScoped<IFileStorage>(c => new S3FileStorage(
                c.GetRequiredService<IOptions<S3FileStorageOptions>>().Value,
                c.GetRequiredService<IAmazonS3>(),
                c.GetRequiredService<IStoredNameProvider>(),
                c.GetRequiredService<IUriResolver>()));
            services.AddScoped<IFileStorageService, FileStorageService>();

            return services;
        }
    }
}
