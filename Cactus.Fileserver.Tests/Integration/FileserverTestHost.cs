using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Simple;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class FileserverTestHost
    {
        
        private static IWebHost _host;
        protected static string BaseUrl => "http://localhost:18047/";
        //protected static TestServer _server;
        //protected static string BaseUrl => _server?.BaseAddress?.ToString();

        [AssemblyInitialize]
        public static async Task StartSimpleFileserver(TestContext context)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", BaseUrl);
            //DO NOT USE: for some reason TestServer hangs if request is more then X bytes, so all image resizing tests fail
            //_server = new TestServer(new WebHostBuilder()
            //    .UseStartup<Startup>()
            //    .UseConfiguration(
            //        new ConfigurationBuilder()
            //            .Build()));
            //_host = _server.Host;
            //await _host.StartAsync();
            _host = WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((ctx, config) =>
                {
                })
                .ConfigureLogging((ctx, logging) =>
                {
                })
                .UseStartup<Startup>()
                .Build();
            await _host.StartAsync();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            var stopped = _host?.StopAsync().Wait(TimeSpan.FromSeconds(5));
            if (stopped ?? false) _host?.Dispose();
        }
         
        protected Task<HttpResponseMessage> Post(string fullFilePath, string mimeType)
        {
            var content = File.OpenRead(fullFilePath);
            return Post(content, Path.GetFileName(fullFilePath), mimeType);
        }

        protected Task<HttpResponseMessage> Post(Stream content, string fileName, string mimeType)
        {
            return Post(new FileUpload
            {
                Content = content,
                MimeType = mimeType,
                FileName = fileName
            });
        }

        protected Task<HttpResponseMessage> Post(params FileUpload[] upload)
        {
            var form = new MultipartFormDataContent();
            foreach (var fileUpload in upload)
            {
                var fileContent = new StreamContent(fileUpload.Content);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(fileUpload.MimeType);
                form.Add(fileContent, "file", fileUpload.FileName);
            }
            var client = new HttpClient();
            return client.PostAsync(BaseUrl + "files", form);
        }

        protected Task<HttpResponseMessage> Get(string url)
        {
            var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
                MaxAutomaticRedirections = 20
            });
            return client.GetAsync(url);
        }

        protected Task<HttpResponseMessage> Delete(string url)
        {
            var client = new HttpClient();
            return client.DeleteAsync(url);
        }

        public class FileUpload
        {
            public Stream Content { get; set; }
            public string FileName { get; set; }
            public string MimeType { get; set; }
        }
    }
}
