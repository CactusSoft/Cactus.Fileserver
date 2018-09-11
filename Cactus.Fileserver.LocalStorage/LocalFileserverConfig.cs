using System;
using Cactus.Fileserver.Config;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Security;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileserverConfig 
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

        public virtual Func<IFileStorageService> FileStorage
        {
            get
            {
                return () =>
                {
                    var baseFilesUri = string.IsNullOrEmpty(Path) || Path == "/"
                        ? BaseUri
                        : new Uri(BaseUri, Path + '/');
                    var fileStorage = new LocalFileStorage(baseFilesUri,
                        new RandomNameProvider {StoreExt = true}, fileStorageFolder);
                    var metaStorage = new LocalMetaInfoStorage(metaStorageFolder);
                    return new FileStorageService(metaStorage, fileStorage, SecurityManager());
                };
            }
        }

        public Func<FileProcessorDelegate> NewFilePipeline { get; set; }

        //public virtual IApplicationBuilder PostPipeline(IApplicationBuilder app)
        //{
        //    //app.UseMiddleware<AddFileHandler>()
        //    //builder.Run(async context =>
        //    //{
        //    //    var handler =
        //    //        new AddFileHandler<T>(
        //    //            builder.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory,
        //    //            config.NewFilePipeline());
        //    //    await handler.Invoke(context);
        //    //});
        //}

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