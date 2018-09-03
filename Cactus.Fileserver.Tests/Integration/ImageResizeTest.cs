using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class ImageResizeTest : FileserverTestHost
    {
        [TestMethod]
        public void ImageOriginalStored()
        {
            var filename = "kartman.png";
            var bytes = File.ReadAllBytes(filename);
            var mimetype = "image/png";
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

        [TestMethod]
        public void ImageDynamicallyResized()
        {
            var filename = "kartman.png";
            var bytes = File.ReadAllBytes(filename);
            var mimetype = "image/png";
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
            var get = new RestRequest(Method.GET)
                .AddParameter("width", 200)
                .AddParameter("Height", 200);
            var gerRes = filestorageClient.Execute(get);
            Assert.AreEqual(ResponseStatus.Completed, gerRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.OK, gerRes.StatusCode);
            Assert.AreEqual(6830, gerRes.RawBytes.Length);
            Assert.AreEqual(mimetype, gerRes.ContentType);

            var del = new RestRequest(Method.DELETE);
            var delRes = filestorageClient.Execute(del);
            Assert.AreEqual(ResponseStatus.Completed, delRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode);
        }
    }
}
