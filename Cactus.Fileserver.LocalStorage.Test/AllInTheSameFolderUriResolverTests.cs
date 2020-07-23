using System;
using System.IO;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Cactus.Fileserver.LocalStorage.Test
{
    public class AllInTheSameFolderUriResolverTests
    {
        [Test]
        public void ResolveUriTest()
        {
            var baseUri = "http://some.somewhere/folder";
            var baseFolder = Path.GetTempPath();
            var fileName = "somefile.ext";
            var fullFilePath = Path.Combine(baseFolder, fileName);
            var options = Options.Create<LocalFileStorageOptions>(new LocalFileStorageOptions
            {
                BaseFolder = baseFolder,
                BaseUri = new Uri(baseUri)
            });
            var resolver = new BaseFolderUriResolver(options);

            var res = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + fullFilePath) });
            Assert.AreEqual(baseUri + "/" + fileName, res.ToString());

            fileName = "withoutextension";
            fullFilePath = Path.Combine(baseFolder, fileName);
            res = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + fullFilePath) });
            Assert.AreEqual(baseUri + "/" + fileName, res.ToString());

            Assert.Throws<ArgumentNullException>(() => resolver.ResolveUri(null));
            Assert.Throws<ArgumentNullException>(() => resolver.ResolveUri(new MetaInfo()));
        }
    }
}