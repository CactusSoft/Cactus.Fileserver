using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Simple;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class FileserverTestHost
    {
        protected static string BaseUrl => _server?.BaseAddress?.ToString();
        private static IWebHost _host;
        protected static TestServer _server;

        [AssemblyInitialize]
        public static async Task StartSimpleFileserver(TestContext context)
        {
            _server = new TestServer(new WebHostBuilder()
                .UseStartup<Startup>()
                .UseConfiguration(
                    new ConfigurationBuilder()
                        .Build()));
            _host = _server.Host;
            await _host.StartAsync();
            //Environment.SetEnvironmentVariable("ASPNETCORE_URLS", BaseUrl);
            //if (hostedSrv == null)
            //    hostedSrv = WebHost.CreateDefaultBuilder()
            //        .UseStartup<Startup>()
            //        .Start();
        }

        [AssemblyCleanup]
        public static void ShutdownFileserver()
        {
            _host?.Dispose();
        }

        protected async Task<HttpResponseMessage> PostFile(string filename, string mimeType)
        {
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filename));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            form.Add(fileContent, "file", Path.GetFileName(filename));
            var client = _server.CreateClient();
            return await client.PostAsync(BaseUrl + "files", form);
        }
    }
}
