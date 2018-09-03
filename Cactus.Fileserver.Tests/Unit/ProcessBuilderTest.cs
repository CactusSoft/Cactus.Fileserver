using System.Collections.Generic;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests
{
    [TestClass]
    public class ProcessBuilderTest
    {
        [TestMethod]
        public void AllHandlersAreCalledTest()
        {
            var b = new GenericPipelineBuilder<MetaInfo>();
            b.Use(next => ((request, content, meta) =>
            {
                meta.Extra.Add("handler1", "handler1");
                return next(request, content, meta);
            }));
            b.Use(next => ((request, content, meta) =>
            {
                meta.Extra.Add("handler2", "handler2");
                return next(request, content, meta);
            }));
            b.Use(next => ((request, content, meta) =>
            {
                meta.Extra.Add("handler3", "handler3");
                return next(request, content, meta);
            }));
            var res = b.Run((x, y, z) => Task.FromResult(new MetaInfo(z)))(null, null, new MetaInfo { Extra = new Dictionary<string, string>() }).Result;

            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Extra);
            Assert.AreEqual(3, res.Extra.Count);
            Assert.IsTrue(res.Extra.ContainsKey("handler1"));
            Assert.IsTrue(res.Extra.ContainsKey("handler2"));
            Assert.IsTrue(res.Extra.ContainsKey("handler3"));
        }

        [TestMethod]
        public void FinallizerCalledTest()
        {
            var b = new GenericPipelineBuilder<MetaInfo>();
            var res = b.Run((request, content, fileInfo) => { fileInfo.Extra.Add("finalizer", "FinallizerCalledTest"); return Task.FromResult(new MetaInfo(fileInfo)); })(null, null, new MetaInfo { Extra = new Dictionary<string, string>() }).Result;

            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Extra);
            Assert.IsTrue(res.Extra.ContainsKey("finalizer"));
            Assert.AreEqual("FinallizerCalledTest", res.Extra["finalizer"]);
        }
    }
}
