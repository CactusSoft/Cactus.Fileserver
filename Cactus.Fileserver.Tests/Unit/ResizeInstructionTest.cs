using Cactus.Fileserver.ImageResizer.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class ResizeInstructionTest
    {
        [TestMethod]
        public void JoinDefaultTest()
        {
            var a = new ResizeInstructions
            {
                KeepAspectRatio = true,
                Width = 200,
                Height = 100
            };
            var b = new ResizeInstructions
            {
                Width = 100,
                KeepAspectRatio = false
            };
            b.Join(a);
            Assert.AreEqual(100, b.Width);
            Assert.AreEqual(100, b.Height);
            Assert.IsTrue(b.KeepAspectRatio.HasValue);
            Assert.IsFalse(b.KeepAspectRatio.Value);
        }

        [TestMethod]
        public void JoinManatoryTest()
        {
            var a = new ResizeInstructions
            {
                KeepAspectRatio = true,
                Width = 200,
                Height = 100,
                MaxWidth = 800,
                MaxHeight = 800
            };
            var b = new ResizeInstructions
            {
                Width = 100,
                KeepAspectRatio = false,
                MaxWidth = 500
            };
            b.Join(a, true);
            Assert.AreEqual(200, b.Width);
            Assert.AreEqual(100, b.Height);
            Assert.AreEqual(800, b.MaxWidth);
            Assert.AreEqual(800, b.MaxHeight);
            Assert.IsTrue(b.KeepAspectRatio.HasValue);
            Assert.IsTrue(b.KeepAspectRatio.Value);
        }

        [TestMethod]
        public void QuerySizeParseTest()
        {
            var q = new QueryString("?width=200&Height=300");
            var res = new ResizeInstructions(q);

            Assert.AreEqual(200, res.Width);
            Assert.AreEqual(300, res.Height);
        }
    }
}
