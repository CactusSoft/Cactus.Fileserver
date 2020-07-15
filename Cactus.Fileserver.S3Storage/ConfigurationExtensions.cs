using System;
using Amazon;
using Cactus.Fileserver.Config;
using Cactus.Fileserver.LocalStorage;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.S3Storage
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddS3Fileserver(this IServiceCollection services, Uri baseFileserverUri, string bucketName, string awsSecretKey, string awsAccessKey, RegionEndpoint regionEndpoint, Func<IServiceProvider, string> metaStorageFolder, Func<IServiceProvider, FileProcessorDelegate> pipeline = null)
        {
            services.AddSingleton<IMetaInfoStorage>(c => new LocalMetaInfoStorage(metaStorageFolder(c),c.GetRequiredService<ILogger<LocalMetaInfoStorage>>()));
            services.AddSingleton<IStoredNameProvider, RandomNameProvider>();
            services.AddSingleton<IFileStorage>(c => new S3FileStorage(bucketName, regionEndpoint, awsAccessKey, awsSecretKey, baseFileserverUri, c.GetRequiredService<IStoredNameProvider>()));
            services.AddSingleton<IFileStorageService, FileStorageService>();
            services.AddSingleton(c => pipeline != null ? pipeline(c) :
                new PipelineBuilder()
                .UseMultipartContent()
                .ExtractFileinfo()
                .Store(c.GetRequiredService<IFileStorageService>()));
            return services;
        }

        public static IApplicationBuilder UseS3Fileserver(this IApplicationBuilder app, string storageFolder)
        {
            return app
                .UseGetFile(b => b
                        .Run(async context =>
                        {
                            // S3 storage means that you will get your files right from the S3.
                            // Maybe later it makes sense to implement redirection here.
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("GET to S3 storage directly is the way of jedi");
                        }))
                .UseAddFile()
                .UseDelFile();
        }
    }

}
