using System;
using System.IO;
using System.Linq;
using System.Net;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.ImageResizer.Core;
using Cactus.Fileserver.ImageResizer.Core.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using RestSharp.Serializers.Newtonsoft.Json;
using RestRequest = RestSharp.RestRequest;

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

        [TestMethod]
        public void ImageDynamicallyResizedMetadataCheck()
        {
            var filename = "kartman.png";
            var bytes = File.ReadAllBytes(filename);
            var mimetype = "image/png";
            var restClient = new RestClient(BaseUrl);
            restClient.AddHandler("application/json", NewtonsoftJsonSerializer.Default);
            var post = new RestRequest(Method.POST);
            post.AddFileBytes(filename, bytes, filename, mimetype);
            var postRes = restClient.Execute<MetaInfo>(post);
            LogRequest(restClient, post, postRes, 0);

            Assert.AreEqual(ResponseStatus.Completed, postRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            Assert.IsNotNull(postRes.Data);
            Assert.AreEqual(filename, postRes.Data.OriginalName);
            var location = postRes.Headers.First(e => e.Name.Equals("Location")).Value.ToString();
            Assert.IsNotNull(location);

            // Get and check 200x200
            var instructions200 = new Instructions {Width = 200, Height = 200};
            var get = new RestRequest(location, Method.GET)
                .AddParameter("width", instructions200.Width)
                .AddParameter("Height", instructions200.Height);
            var getRes = restClient.Execute(get);
            Assert.AreEqual(ResponseStatus.Completed, getRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode);
            Assert.AreEqual(6830, getRes.RawBytes.Length);
            Assert.AreEqual(mimetype, getRes.ContentType);

            var getMeta = new RestRequest(new Uri(location + ".json"), Method.GET);
            var meta = restClient.Execute<MetaInfo>(getMeta);
            Assert.IsNotNull(meta.Data);
            Assert.IsNotNull(meta.Data.Extra);
            Assert.IsNotNull(meta.Data.Extra[instructions200.GetSizeKey()]);

            // Get and check 300x300
            var instructions300 = new Instructions { Width = 300, Height = 300 };
            get = new RestRequest(location, Method.GET)
                .AddParameter("width", instructions300.Width)
                .AddParameter("Height", instructions300.Height);
            getRes = restClient.Execute(get);
            Assert.AreEqual(ResponseStatus.Completed, getRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode);
            Assert.AreEqual(11963, getRes.RawBytes.Length);
            Assert.AreEqual(mimetype, getRes.ContentType);

            getMeta = new RestRequest(new Uri(location + ".json"), Method.GET);
            meta = restClient.Execute<MetaInfo>(getMeta);
            Assert.IsNotNull(meta.Data);
            Assert.IsNotNull(meta.Data.Extra);
            Assert.IsNotNull(meta.Data.Extra[instructions200.GetSizeKey()]);
            Assert.IsNotNull(meta.Data.Extra[instructions300.GetSizeKey()]);

            var del = new RestRequest(location, Method.DELETE);
            var delRes = restClient.Execute(del);
            Assert.AreEqual(ResponseStatus.Completed, delRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode);
        }
    }
}
