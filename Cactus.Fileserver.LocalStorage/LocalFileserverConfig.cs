using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Config;
using Cactus.Fileserver.Core.Security;
using Cactus.Fileserver.Core.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using IFileInfo = Cactus.Fileserver.Core.Model.IFileInfo;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileserverConfig<TMeta> : IFileserverConfig<TMeta> where TMeta : IFileInfo
    {
        private readonly string fileStorageFolder;
        private readonly string metaStorageFolder;

        public LocalFileserverConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri, string path)
        {
            this.fileStorageFolder = fileStorageFolder;
            this.metaStorageFolder = metaStorageFolder;
            BaseUri = baseUri;
            Path = path;
            this.fileStorageFolder = fileStorageFolder;
            SecurityManager = () => new NothingCheckSecurityManager();
        }

        public Uri BaseUri { get; }

        public Func<ISecurityManager> SecurityManager { get; set; }

        public string Path { get; }

        public Func<IFileStorageService<TMeta>> FileStorage
        {
            get
            {
                return () =>
                {
                    var baseFilesUri = string.IsNullOrEmpty(Path) || Path == "/"
                        ? BaseUri
                        : new Uri(BaseUri, Path + '/');
                    var fileStorage = new LocalFileStorage<TMeta>(baseFilesUri,
                        new RandomNameProvider<TMeta> {StoreExt = true}, fileStorageFolder);
                    var metaStorage = new LocalMetaInfoStorage<TMeta>(metaStorageFolder);
                    return new FileStorageService<TMeta>(metaStorage, fileStorage, SecurityManager());
                };
            }
        }

        public Func<Func<HttpRequest, HttpContent, TMeta, Task<TMeta>>> NewFilePipeline { get; set; }

        public virtual IApplicationBuilder GetPipeline(IApplicationBuilder app)
        {
            return app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(fileStorageFolder),
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true
            });
        }
    }
}