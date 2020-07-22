using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Cactus.Fileserver.Aspnet.Dto;
using Cactus.Fileserver.ImageResizer.Utils;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Simple;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Tests.Integration
{
    [TestClass]
    public class ImageResizeTest: FileserverTestHost
    {
        [TestMethod]
        public async Task ImageOriginalStored()
        {
            var filename = "kartman.png";
            var postRes = await Post(filename, "image/png");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            Assert.IsNotNull(postRes.Headers.Location);

            var getRes = await Get(postRes.Headers.Location.ToString());
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.AreEqual((new FileInfo(filename)).Length, (await getRes.Content.ReadAsByteArrayAsync()).Length);
            Assert.AreEqual("image/png", getRes.Content.Headers.ContentType.MediaType);

            var delRes = await Delete(postRes.Headers.Location.ToString());
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task ImageDynamicallyResized()
        {
            var filename = "kartman.png";
            var postRes = await Post(filename, "image/png");

            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            Assert.IsNotNull(postRes.Headers.Location);
            var originFileLocation = postRes.Headers.Location.ToString();

            var resizedUrl = originFileLocation + "?width=200&Height=200";
            var getRes = await Get(resizedUrl);
            Assert.AreEqual(HttpStatusCode.MovedPermanently, getRes.StatusCode);
            Assert.IsNotNull(getRes.Headers.Location);
            var resizedFileLocation = getRes.Headers.Location.ToString();

            getRes = await Get(resizedUrl);
            Assert.AreEqual(HttpStatusCode.MovedPermanently, getRes.StatusCode);
            Assert.AreEqual(resizedFileLocation, getRes.Headers.Location.ToString(), "No resizing should occurred second time, the reference should stay the same");

            getRes = await Get(resizedFileLocation);
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.IsTrue((new FileInfo(filename)).Length > (await getRes.Content.ReadAsByteArrayAsync()).Length);
            Assert.AreEqual("image/png", getRes.Content.Headers.ContentType.MediaType);

            var delRes = await Delete(resizedFileLocation);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
            delRes = await Delete(originFileLocation);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        [TestMethod]
        public async Task ImageDynamicallyResizedMetadataCheck()
        {
            var filename = "kartman.png";
            var instructions200 = new ResizeInstructions { Width = 200, Height = 200 };
            var postRes = await Post(filename, "image/png");
            Assert.IsTrue(postRes.IsSuccessStatusCode, postRes.ToString());
            Assert.AreEqual(HttpStatusCode.Created, postRes.StatusCode);
            Assert.IsNotNull(postRes.Headers.Location);
            Assert.IsNotNull(postRes.Content);
            var postResponse = JsonConvert.DeserializeObject<ResponseDto[]>(await postRes.Content.ReadAsStringAsync());
            Assert.AreEqual(filename, postResponse.First().OriginalName);

            // Get and check 200x200
            var originLocation = postRes.Headers.Location.ToString();
            var resizedUrl = originLocation + "?width=200&Height=200";
            var getRes = await Get(resizedUrl);
            Assert.AreEqual(HttpStatusCode.MovedPermanently, getRes.StatusCode);
            Assert.IsNotNull(getRes.Headers.Location);
            var resizedFileLocation = getRes.Headers.Location.ToString();

            getRes = await Get(resizedFileLocation);
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.AreEqual(HttpStatusCode.OK, getRes.StatusCode, getRes.ToString());
            Assert.IsTrue((new FileInfo(filename)).Length > (await getRes.Content.ReadAsByteArrayAsync()).Length);

            getRes = await Get(resizedFileLocation + ".json");
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.IsNotNull(getRes.Content);
            var meta = JsonConvert.DeserializeObject<MetaInfo>(await getRes.Content.ReadAsStringAsync());
            Assert.AreEqual(originLocation, meta.Origin.ToString());

            getRes = await Get(originLocation + ".json");
            Assert.IsTrue(getRes.IsSuccessStatusCode, getRes.ToString());
            Assert.IsNotNull(getRes.Content);
            meta = JsonConvert.DeserializeObject<MetaInfo>(await getRes.Content.ReadAsStringAsync());
            Assert.IsNotNull(meta.Extra);
            Assert.IsNotNull(meta.Extra[instructions200.BuildSizeKey()]);

            var delRes = await Delete(originLocation);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());

            delRes = await Delete(resizedFileLocation);
            Assert.IsTrue(delRes.IsSuccessStatusCode, delRes.ToString());
            Assert.AreEqual(HttpStatusCode.NoContent, delRes.StatusCode, delRes.ToString());
        }

        
    }
}
