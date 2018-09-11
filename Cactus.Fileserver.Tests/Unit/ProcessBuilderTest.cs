using System.Collections.Generic;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class ProcessBuilderTest
    {
        [TestMethod]
        public void AllHandlersAreCalledTest()
        {
            var b = new PipelineBuilder();
            b.Use(next => ((request, content, stream, meta) =>
            {
                meta.Extra.Add("handler1", "handler1");
                return next(request, content, stream, meta);
            }));
            b.Use(next => ((request, content, stream, meta) =>
            {
                meta.Extra.Add("handler2", "handler2");
                return next(request, content, stream, meta);
            }));
            b.Use(next => ((request, content, stream, meta) =>
            {
                meta.Extra.Add("handler3", "handler3");
                return next(request, content, stream, meta);
            }));
            var res = b.Run((request, content, stream, meta) => Task.FromResult(meta as MetaInfo))(null, null, null, new MetaInfo()).Result;

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
            var b = new PipelineBuilder();
            var res = b.Run((request, content, stream, meta) => { meta.Extra.Add("finalizer", "FinallizerCalledTest"); return Task.FromResult(meta as MetaInfo); })(null, null, null, new MetaInfo { Extra = new Dictionary<string, string>() }).Result;

            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Extra);
            Assert.IsTrue(res.Extra.ContainsKey("finalizer"));
            Assert.AreEqual("FinallizerCalledTest", res.Extra["finalizer"]);
        }
    }
}
