using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cactus.Fileserver.ImageResizer;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cactus.Fileserver.Tests.Unit
{
    [TestClass]
    public class PipelinBuilderExtensionsTest
    {
        [TestMethod]
        public async Task AcceptOnlyImageContentFailTest()
        {
            using (var notAnImage = new MemoryStream(Encoding.ASCII.GetBytes("not an image binary file")))
            {
                var handler = new PipelineBuilder()
                    .AcceptOnlyImageContent()
                    .Run((request, content, stream, info) => Task.FromResult<MetaInfo>(null));

                var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await handler(null, null, notAnImage, null));
                Assert.AreEqual(ConfigurationExtensions.NotAnImageExceptionMessage, ex.Message);
            }
        }

        [TestMethod]
        public async Task AcceptOnlyImageContentSuccessTest()
        {
            var filename = "kartman.png";
            using (var image = File.OpenRead(filename))
            {
                var handler = new PipelineBuilder()
                    .AcceptOnlyImageContent()
                    .Run((request, content, stream, info) => Task.FromResult(new MetaInfo { OriginalName = filename }));

                var res = await handler(null, null, image, null);
                Assert.IsNotNull(res);
                Assert.AreEqual(filename, res.OriginalName);
            }
        }
    }
}
