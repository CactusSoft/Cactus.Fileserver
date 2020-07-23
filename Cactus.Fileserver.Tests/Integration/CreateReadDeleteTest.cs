using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public async Task SingleTextCrudTest()
        {
            var content = "Hello world!";
            var postRes = await Post(new StringContent(content));
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);

            var getRes = await Get(location);
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual(content.Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);

            var delRes = await Delete(location);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task SingleImageCrudTest()
        {
            var file = "kartman.png";
            var content = File.OpenRead(file);
            var httpContent = new StreamContent(content);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            var postRes = await Post(httpContent);
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);

            var getRes = await Get(location);
            if (getRes.StatusCode == HttpStatusCode.MovedPermanently)
            {
                getRes = await Get(getRes.Headers.Location.ToString());
            }
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual(content.Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);

            var delRes = await Delete(location);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task MultipartCrudTest()
        {
            var filename = "test.txt";
            var fileContent = Encoding.ASCII.GetBytes("Hello world!");
            var fileStream = new MemoryStream(fileContent);
            var postRes = await PostMultipart(fileStream, filename, "plain/text");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);

            var getRes = await Get(location);
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual(fileContent.Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);

            var delRes = await Delete(location);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task MultipleUploadTest()
        {
            var files = new[]
            {
                new FileUpload
                {
                    FileName = "test1.txt",
                    MimeType = "text/plain",
                    Content = new MemoryStream(Encoding.ASCII.GetBytes("Hello world!"))
                },
                new FileUpload
                {
                    //FileName = "kartman.png",
                    //Content = File.OpenRead("kartman.png"),
                    //MimeType = "image/png"
                    FileName = "test2.txt",
                    MimeType = "text/plain",
                    Content = new MemoryStream(Encoding.ASCII.GetBytes("second file content"))
                },
                new FileUpload
                {
                    FileName = "test3.txt",
                    MimeType = "text/plain",
                    Content = new MemoryStream(Encoding.ASCII.GetBytes("Hello world!!!!"))
                }
            };

            var postRes = await PostMultipart(files);
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);
            Assert.IsNotNull(postRes.Content);
            var metaData = JsonConvert.DeserializeObject<MetaInfo[]>(await postRes.Content.ReadAsStringAsync());
            Assert.AreEqual(files[0].FileName, metaData[0].OriginalName);
            Assert.AreEqual(files[1].FileName, metaData[1].OriginalName);
            Assert.AreEqual(files[2].FileName, metaData[2].OriginalName);

            foreach (var meta in metaData)
            {
                var delRes = await Delete(meta.Uri.ToString());
                Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
                Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
            }
        }

        [TestMethod]
        public async Task FileMetadataTest()
        {
            var filename = "test.txt";
            var fileContent = Encoding.ASCII.GetBytes("Hello meta world!");
            await using var fileStream = new MemoryStream(fileContent);
            var postRes = await PostMultipart(fileStream, filename, "plain/text");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            var location = postRes.Headers.Location.ToString();
            Assert.IsNotNull(location);

            var getRes = await Get(location + ".json");
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.IsNotNull(getRes.Content);
            var metaData = JsonConvert.DeserializeObject<MetaInfo[]>(await postRes.Content.ReadAsStringAsync());
            Assert.AreEqual(filename, metaData.First().OriginalName);

            var delRes = await Delete(location);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }
    }
}
