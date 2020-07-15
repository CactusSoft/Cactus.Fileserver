using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class CreateReadDeleteTest : FileserverTestHost
    {
        [TestMethod]
        public async Task FileCrudTest()
        {
            var filename = "kartman.png";
            var postRes = await PostFile(filename, "octet/stream");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);
            
            var getRes = await _server.CreateRequest(location).GetAsync();
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual((new FileInfo(filename)).Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);

            var delRes = await _server.CreateRequest(location).SendAsync(HttpMethod.Delete.Method);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task FileMetadataTest()
        {
            var filename = "kartman.png";
            var postRes = await PostFile(filename, "octet/stream");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);

            var getRes = await _server.CreateRequest(location+".json").GetAsync();
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.IsNotNull(getRes.Content);
            var metaData = JsonConvert.DeserializeObject<MetaInfo>(await postRes.Content.ReadAsStringAsync());
            Assert.AreEqual(filename, metaData.OriginalName);

            var delRes = await _server.CreateRequest(location).SendAsync(HttpMethod.Delete.Method);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }
    }
}
