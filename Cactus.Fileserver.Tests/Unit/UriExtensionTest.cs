using System;
using Cactus.Fileserver.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class UriExtensionTest
    {
        [TestMethod]
        public void GetResourceTest()
        {
            Assert.AreEqual("file.ext", new Uri("http://srv.co/file.ext").GetResource());
            Assert.AreEqual("file", new Uri("http://srv.co/file").GetResource());
            Assert.AreEqual("", new Uri("http://srv.co").GetResource());
            Assert.AreEqual("", new Uri("http://srv.co/folder/").GetResource());
        }
    }
}
