using System;
using Cactus.Fileserver.Asp5.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace LocalFileserver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseErrorTrace()
               .UseFileserver(
                    new LocalFileserverConfig(
                        @"d:\temp",
                        new Uri("http://localhost:38420"),
                        @"/file"
                    )
                );
        }
    }

    public static class TraceMiddleware
    {
        public static IApplicationBuilder UseErrorTrace(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke();
                }
                catch (Exception e)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(e, Formatting.Indented));
                }
            });
            return app;
        }
    }
}
