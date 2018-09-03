using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Cactus.Fileserver.Simple;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RestSharp;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class CreateReadDeleteTest : FileserverTestHost
    {
        [TestMethod]
        public void DummyFileTest()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var filename = "something.dummy";
            var mimetype = "application/octet-stream";
            var restClient = new RestClient(BaseUrl);
            var post = new RestRequest(Method.POST);
            post.AddFileBytes(filename, bytes, filename, mimetype);
            var postRes = restClient.Execute(post);
            LogRequest(restClient, post, postRes, 0);

            Assert.AreEqual(ResponseStatus.Completed, postRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.First(e => e.Name.Equals("Location")).Value.ToString();
            Assert.IsNotNull(location);

            var uri = new Uri(location);
            var filestorageClient = new RestClient(uri);
            var get = new RestRequest(Method.GET);
            var gerRes = filestorageClient.Execute(get);
            Assert.AreEqual(ResponseStatus.Completed, gerRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.OK, gerRes.StatusCode);
            Assert.AreEqual(bytes.Length, gerRes.RawBytes.Length);
            Assert.AreEqual(mimetype, gerRes.ContentType);

            var del = new RestRequest(Method.DELETE);
            var delRes = filestorageClient.Execute(del);
            Assert.AreEqual(ResponseStatus.Completed, delRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode);
        }
    }
}
