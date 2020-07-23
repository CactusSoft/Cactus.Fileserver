using System;
using Amazon;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileserverBuilder
    {

    }
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddS3Fileserver(this IServiceCollection services)
        {
            
            return services;
        }

        public static IApplicationBuilder UseS3Fileserver(this IApplicationBuilder app)
        {
            return app;
        }
    }

}
