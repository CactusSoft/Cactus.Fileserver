using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace Cactus.Fileserver.LocalStorage.Test
{
    public class LocalFileStorageTests
    {
        [Test]
        public async Task CrudTest()
        {
            //Arrange
            var baseUri = "http://some.somewhere/folder";
            var baseFolder = Path.GetTempPath();
            var fileName = Path.GetRandomFileName();
            var metaInfo = new MetaInfo
            {
                OriginalName = fileName,
                Extra = new Dictionary<string, string> { { "key", "value" } },
                MimeType = "octer/stream",
            };

            var nameGeneratorMock = new Mock<IStoredNameProvider>();
            nameGeneratorMock.Setup(e => e.GetName(It.IsAny<IMetaInfo>())).Returns<IMetaInfo>(meta => meta.OriginalName);

            var uriResolverMock = new Mock<IUriResolver>();
            uriResolverMock.Setup(e => e.ResolvePath(It.IsAny<IMetaInfo>())).Returns(baseFolder);
            uriResolverMock.Setup(e => e.ResolveUri(It.IsAny<IMetaInfo>()))
                .Returns<IMetaInfo>(meta => new Uri(baseUri + '/' + meta.InternalUri.GetResource()));

            var storage = new LocalFileStorage(nameGeneratorMock.Object, uriResolverMock.Object, NullLogger.Instance);
            await using (var stream = new MemoryStream())
            {
                stream.Write(Encoding.ASCII.GetBytes("hello world"));
                stream.Seek(0, 0);

                //Act
                await storage.Add(stream, metaInfo);
                Assert.NotNull(metaInfo.InternalUri);
                Assert.NotNull(metaInfo.Uri);
                Assert.IsTrue(File.Exists(Path.Combine(baseFolder, fileName)));
                nameGeneratorMock.VerifyAll();

                await using var resStream = await storage.Get(metaInfo);
                Assert.IsNotNull(resStream);
                Assert.AreEqual(stream.Length, resStream.Length);
                Assert.IsTrue(File.Exists(Path.Combine(baseFolder, fileName)));
            }

            await storage.Delete(metaInfo);
            Assert.IsFalse(File.Exists(Path.Combine(baseFolder, fileName)));
            uriResolverMock.VerifyAll();
        }
    }
}