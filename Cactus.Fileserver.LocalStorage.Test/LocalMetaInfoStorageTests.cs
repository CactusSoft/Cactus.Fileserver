using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Cactus.Fileserver.LocalStorage.Test
{
    public class LocalMetaInfoStorageTests
    {
        [Test]
        public async Task CrudTest()
        {
            var metafileExt = ".json";
            var baseFolder = Path.GetTempPath();
            var fileName = Path.GetRandomFileName();
            var metaStorage = new LocalMetaInfoStorage(baseFolder, NullLogger.Instance, metafileExt);
            var metaInfo = new MetaInfo
            {
                InternalUri = new Uri("file://" + baseFolder + '/' + fileName),
                Uri = new Uri("http://some.somewhere/folder/" + fileName),
                Extra = new Dictionary<string, string> { { "key", "value" } },
                MimeType = "octer/stream",
            };

            await metaStorage.Add(metaInfo);
            Assert.IsTrue(File.Exists(Path.Combine(baseFolder, fileName + metafileExt)));

            metaInfo.Extra.Add("updated", "-");
            await metaStorage.Update(metaInfo);
            Assert.IsTrue(File.Exists(Path.Combine(baseFolder, fileName + metafileExt)));
            
            var metaInfoReceived = await metaStorage.Get<MetaInfo>(metaInfo.Uri);
            Assert.IsTrue(metaInfoReceived.Extra.ContainsKey("updated"));

            await metaStorage.Delete(metaInfo.Uri);
            Assert.IsFalse(File.Exists(Path.Combine(baseFolder, fileName + metafileExt)));
        }
    }
}