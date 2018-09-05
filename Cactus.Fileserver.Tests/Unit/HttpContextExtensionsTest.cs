using System;
using System.Collections.Generic;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class HttpContextExtensionsTest
    {
        [TestMethod]
        public void AddOrUpdateContextTest()
        {
            var dic = new Dictionary<object, object>();
            var ctx = new FileContext
            {
                IncomeFileInfo = new MetaInfo { OriginalName = "test"}
            };

            Assert.IsNull(HttpContextExtensions.ExtractFileContext(dic));
            HttpContextExtensions.AddOrUpdateFileContext(dic, ctx);

            Assert.IsNotNull(HttpContextExtensions.ExtractFileContext(dic));
            Assert.AreEqual(ctx, HttpContextExtensions.ExtractFileContext(dic));
           
            var ctx2 = new FileContext
            {
                IncomeFileInfo = new MetaInfo { OriginalName = "test2" }
            };
            HttpContextExtensions.AddOrUpdateFileContext(dic, ctx2);
            Assert.IsNotNull(HttpContextExtensions.ExtractFileContext(dic));
            Assert.AreEqual(ctx2, HttpContextExtensions.ExtractFileContext(dic));
        }
    }
}
