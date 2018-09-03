using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests
{
    [TestClass]
    public class RandomNameProviderTest
    {
        [TestMethod]
        public void TestUnique()
        {
            var file = new MetaInfo();
            var p = new RandomNameProvider<MetaInfo>();
            var name1 = p.GetName(file);
            var name2 = p.GetName(file);
            var name3 = p.Regenerate(file, name2);

            Assert.IsFalse(string.IsNullOrEmpty(name1));
            Assert.IsFalse(string.IsNullOrEmpty(name2));
            Assert.IsFalse(string.IsNullOrEmpty(name3));
            Assert.AreNotEqual(name1, name2);
            Assert.AreNotEqual(name1, name3);
            Assert.AreNotEqual(name2, name3);
        }

        [TestMethod]
        public void ExtensionStoredTest()
        {
            var p = new RandomNameProvider<MetaInfo>() { StoreExt = true };
            var name1 = p.GetName(new MetaInfo { OriginalName = "test.com" });
            var name2 = p.GetName(new MetaInfo { OriginalName = "test." });
            var name3 = p.GetName(new MetaInfo { OriginalName = "test" });
            var name4 = p.GetName(new MetaInfo { OriginalName = "" });
            var name5 = p.GetName(new MetaInfo { OriginalName = null });


            Assert.IsTrue(name1.EndsWith(".com"));
            Assert.IsFalse(name2.EndsWith("."));
            Assert.IsFalse(name2.Contains("test"));
            Assert.IsFalse(name3.EndsWith("."));
            Assert.IsFalse(name3.Contains("test"));
            Assert.IsFalse(string.IsNullOrEmpty(name4));
            Assert.IsFalse(string.IsNullOrEmpty(name5));

            Assert.AreEqual(name4.Length, name5.Length);
            Assert.AreEqual(name3.Length, name4.Length);
            Assert.AreEqual(name2.Length, name3.Length);
        }
    }
}
