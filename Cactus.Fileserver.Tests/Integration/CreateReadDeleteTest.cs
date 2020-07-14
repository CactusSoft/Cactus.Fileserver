using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class CreateReadDeleteTest : FileserverTestHost
    {
        [TestMethod]
        public async Task DummyFileTest()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            var filename = "kartman.png";

            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filename));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filename));
            var client = _server.CreateClient();
            var response = await client.PostAsync(BaseUrl + "files", form);

            Assert.IsTrue(response.IsSuccessStatusCode, response.ToString());
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var location = response.Headers.Location.ToString();
            Assert.IsNotNull(location);


            var getRes = await _server.CreateRequest(location).GetAsync();
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual((new FileInfo(filename)).Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);

            var delRes = await _server.CreateRequest(location).SendAsync(HttpMethod.Delete.Method);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }
    }
}
