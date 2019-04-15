using System.Collections.Specialized;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.ImageResizer.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class ImageResizerTest
    {
        [TestMethod]
        public void GetTargetSizeRatio05Test()
        {
            var nv = new NameValueCollection();
            nv.Add("maxwidth", "1000");
            nv.Add("maxheight", "1000");

            var res = ImageResizerService.GetTargetSize(new Instructions(nv)
            {
                Width = 100,
                Height = 100,
            }, 0.5);
            Assert.AreEqual(50,res.Width);
            Assert.AreEqual(100, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeRatio15Test()
        {
            var nv = new NameValueCollection();
            nv.Add("maxwidth", "1000");
            nv.Add("maxheight", "1000");

            var res = ImageResizerService.GetTargetSize(new Instructions(nv)
            {
                Width = 100,
                Height = 100,
            }, 1.5);
            Assert.AreEqual(100, res.Width);
            Assert.AreEqual(67, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeLimitedMaxTest()
        {
            var nv = new NameValueCollection();
            nv.Add("maxwidth", "1000");
            nv.Add("maxheight", "1000");

            var res = ImageResizerService.GetTargetSize(new Instructions(nv)
            {
                Width = 10000,
                Height = 10000,
            }, 1);
            Assert.AreEqual(1000, res.Width);
            Assert.AreEqual(1000, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeLimitedMaxRatio05Test()
        {
            var nv = new NameValueCollection();
            nv.Add("maxwidth", "1000");
            nv.Add("maxheight", "1000");

            var res = ImageResizerService.GetTargetSize(new Instructions(nv)
            {
                Width = 10000,
                Height = 10000,
            }, 0.5);
            Assert.AreEqual(500, res.Width);
            Assert.AreEqual(1000, res.Height);
        }
    }
}
