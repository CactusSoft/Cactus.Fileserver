using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var restClient = new RestClient(BaseUrl+"/files");
            var post = new RestRequest(Method.POST);
            post.AddFileBytes(filename, bytes, filename, mimetype);
            var postRes = restClient.Execute(post);
            LogRequest(restClient, post, postRes, 0);

            Assert.AreEqual(ResponseStatus.Completed, postRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.First(e => e.Name.Equals("Location")).Value.ToString();
            Assert.IsNotNull(location);

            var get = new RestRequest(location, Method.GET);
            var gerRes = restClient.Execute(get);
            Assert.AreEqual(ResponseStatus.Completed, gerRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.OK, gerRes.StatusCode);
            Assert.AreEqual(bytes.Length, gerRes.RawBytes.Length);
            Assert.AreEqual(mimetype, gerRes.ContentType);

            var del = new RestRequest(location, Method.DELETE);
            var delRes = restClient.Execute(del);
            Assert.AreEqual(ResponseStatus.Completed, delRes.ResponseStatus);
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode);
        }
    }
}
