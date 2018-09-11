using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Cactus.Fileserver.Simple;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class FileserverTestHost
    {
        protected static string BaseUrl = "http://localhost:5000";
        private static IDisposable hostedSrv;

        [AssemblyInitialize]
        public static void StartSimpleFileserver(TestContext context)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", BaseUrl);
            if (hostedSrv == null)
                hostedSrv = WebHost.CreateDefaultBuilder()
                    .UseStartup<Startup>()
                    .Start();
        }

        [AssemblyCleanup]
        public static void ShutdownFileserver()
        {
            hostedSrv?.Dispose();
        }

        protected void LogRequest(RestClient restClient, IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = restClient.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            Trace.Write(string.Format("Request completed in {0} ms, Request: {1}, Response: {2}",
                durationMs,
                JsonConvert.SerializeObject(requestToLog),
                JsonConvert.SerializeObject(responseToLog)));
        }
    }
}
