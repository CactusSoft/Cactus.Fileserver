using System;
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
            Assert.AreEqual("file", new Uri("http://srv.co//file").GetResource());
            Assert.AreEqual("file", new Uri("http://srv.co/folder/folder/file").GetResource());
            Assert.AreEqual("file.ext", new Uri("http://srv.co/folder/folder/file.ext").GetResource());
            Assert.AreEqual("file.ext", new Uri("http://srv.co//folder/folder/file.ext").GetResource());
            Assert.AreEqual("", new Uri("http://srv.co").GetResource());
            Assert.AreEqual("", new Uri("http://srv.co/folder/").GetResource());
        }

        [TestMethod]
        public void GetFolderTest()
        {
            Assert.AreEqual(new Uri("http://srv.co/"), new Uri("http://srv.co/file.ext").GetFolder());
            Assert.AreEqual(new Uri("http://srv.co/"), new Uri("http://srv.co//file.ext").GetFolder());
            Assert.AreEqual(new Uri("http://srv.co/folder"), new Uri("http://srv.co/folder/file.ext").GetFolder());
            Assert.AreEqual(new Uri("http://srv.co/folder1/folder2"), new Uri("http://srv.co/folder1/folder2/file.ext").GetFolder());
            Assert.AreEqual(new Uri("http://srv.co//folder"), new Uri("http://srv.co//folder/file.ext").GetFolder());
            Assert.AreEqual(new Uri("http://srv.co/folder/folder"), new Uri("http://srv.co/folder/folder/file.ext").GetFolder());
        }
    }
}
