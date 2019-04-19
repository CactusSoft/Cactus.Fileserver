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
            var res = ImageResizerService.GetTargetSize(new ResizeInstructions
            {
                Width = 100,
                Height = 100,
                KeepAspectRatio = true
            }, 0.5);
            Assert.AreEqual(50, res.Width);
            Assert.AreEqual(100, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeRatio15Test()
        {
            var res = ImageResizerService.GetTargetSize(new ResizeInstructions
            {
                Width = 100,
                Height = 100,
                KeepAspectRatio = true
            }, 1.5);
            Assert.AreEqual(100, res.Width);
            Assert.AreEqual(67, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeLimitedMaxTest()
        {
            var inst = new ResizeInstructions
            {
                MaxWidth = 100,
                MaxHeight = 100,
                Width = 10000,
                Height = 10000,
                KeepAspectRatio = true
            };
            var res = ImageResizerService.GetTargetSize(inst, 1);
            Assert.AreEqual(inst.MaxWidth, res.Width);
            Assert.AreEqual(inst.MaxHeight, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeLimitedMaxRatio05Test()
        {
            var inst = new ResizeInstructions
            {
                MaxWidth = 100,
                MaxHeight = 100,
                Width = 10000,
                Height = 10000,
                KeepAspectRatio = true
            };
            var res = ImageResizerService.GetTargetSize(inst, 0.5);
            Assert.AreEqual(50, res.Width);
            Assert.AreEqual(100, res.Height);
        }

        [TestMethod]
        public void GetTargetSizeNotZeroTest()
        {
            var res = ImageResizerService.GetTargetSize(new ResizeInstructions
            {
                Width = 100,
                Height = null,
                KeepAspectRatio = true
            }, 1);
            Assert.AreEqual(100, res.Width);
            Assert.AreEqual(100, res.Height);
        }

        [TestMethod]
        public void BuildSizeKeyTest()
        {
            Assert.AreEqual("alt_size_800x600", new ResizeInstructions
            {
                Width = 800,
                Height = 600
            }.BuildSizeKey());
            Assert.AreEqual("alt_size_800x-", new ResizeInstructions
            {
                Width = 800
            }.BuildSizeKey());
            Assert.AreEqual("alt_size_-x-", new ResizeInstructions().BuildSizeKey());
        }
    }
}
