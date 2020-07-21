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

        protected Task<HttpResponseMessage> PostFile(string fullFilePath, string mimeType)
        {
            using var content = File.OpenRead(fullFilePath);
            return PostFile(content, Path.GetFileName(fullFilePath), mimeType);
        }

        protected Task<HttpResponseMessage> PostFile(Stream content, string fileName, string mimeType)
        {
            return PostFiles(new FileUpload
            {
                Content = content,
                MimeType = mimeType,
                FileName = fileName
            });
        }

        protected Task<HttpResponseMessage> PostFiles(params FileUpload[] upload)
        {
            using var form = new MultipartFormDataContent();
            foreach (var fileUpload in upload)
            {
                var fileContent = new StreamContent(fileUpload.Content);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(fileUpload.MimeType);
                form.Add(fileContent, "file", fileUpload.FileName);
            }
            var client = _server.CreateClient();
            return client.PostAsync(BaseUrl + "files", form);
        }

        public class FileUpload
        {
            public Stream Content { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
        }
    }
}
