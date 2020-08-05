using System;
using System.IO;
using Cactus.Fileserver.LocalStorage.Config;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Cactus.Fileserver.LocalStorage.Test
{
    [TestFixture]
    public class FolderUriResolverTest
    {
        [Test]
        public void ResolvePathTest()
        {
            var baseUrl = "http://localhost:5000";
            var options = Options.Create(new LocalFileStorageOptions
            {
                BaseFolder = Path.GetTempPath(),
                BaseUri = new Uri(baseUrl)
            });

            var resolver = new SubfolderUriResolver(options);

            var path = resolver.ResolvePath(new MetaInfo { Uri = options.Value.BaseUri });
            Assert.AreEqual(options.Value.BaseFolder, path);

            path = resolver.ResolvePath(new MetaInfo { Uri = new Uri(baseUrl + "/folder1") });
            Assert.AreEqual(Path.Combine(options.Value.BaseFolder, "folder1"), path);

            path = resolver.ResolvePath(new MetaInfo { Uri = new Uri(baseUrl + "/folder1/folder2") });
            Assert.AreEqual(Path.Combine(options.Value.BaseFolder, "folder1", "folder2"), path);

            path = resolver.ResolvePath(new MetaInfo { Uri = new Uri(baseUrl + "/folder1/folder2?param=value&x=y") });
            Assert.AreEqual(Path.Combine(options.Value.BaseFolder, "folder1", "folder2"), path);
        }

        [Test]
        public void ResolveUriTest()
        {
            var baseUrl = "http://localhost:5000";
            var options = Options.Create(new LocalFileStorageOptions
            {
                BaseFolder = Path.GetTempPath(),
                BaseUri = new Uri(baseUrl)
            });

            var resolver = new SubfolderUriResolver(options);

            var uri = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + Path.Combine(options.Value.BaseFolder, "file.ext")) });
            Assert.AreEqual(new Uri(baseUrl + "/file.ext"), uri);

            uri = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + Path.Combine(options.Value.BaseFolder, "folder1", "file.ext")) });
            Assert.AreEqual(new Uri(baseUrl + "/folder1/file.ext"), uri);

            uri = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + Path.Combine(options.Value.BaseFolder, "folder1", "folder2", "file.ext")) });
            Assert.AreEqual(new Uri(baseUrl + "/folder1/folder2/file.ext"), uri);

            uri = resolver.ResolveUri(new MetaInfo { InternalUri = new Uri("file://" + Path.Combine(options.Value.BaseFolder, "folder1", "folder2", "file.ext")+"?query=val") });
            Assert.AreEqual(new Uri(baseUrl + "/folder1/folder2/file.ext"), uri);

        }
    }
}
