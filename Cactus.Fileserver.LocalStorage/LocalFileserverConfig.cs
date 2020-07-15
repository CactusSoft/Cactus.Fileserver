using System;
using Cactus.Fileserver.Pipeline;
using Cactus.Fileserver.Security;
using Cactus.Fileserver.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileserverConfig 
    {
        private readonly string _fileStorageFolder;
        private readonly string _metaStorageFolder;
        private readonly ILoggerFactory _loggerFactory;

        public LocalFileserverConfig(string fileStorageFolder, string metaStorageFolder, Uri baseUri, string path, ILoggerFactory loggerFactory)
        {
            this._fileStorageFolder = fileStorageFolder;
            this._metaStorageFolder = metaStorageFolder;
            _loggerFactory = loggerFactory;
            BaseUri = baseUri;
            Path = path;
            this._fileStorageFolder = fileStorageFolder;
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
                        new RandomNameProvider {StoreExt = true},_loggerFactory.CreateLogger(typeof(LocalFileStorage)), _fileStorageFolder);
                    var metaStorage = new LocalMetaInfoStorage(_metaStorageFolder, _loggerFactory.CreateLogger(typeof(LocalMetaInfoStorage)));
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
                FileProvider = new PhysicalFileProvider(_fileStorageFolder),
                DefaultContentType = "application/octet-stream",
                ServeUnknownFileTypes = true
            });
        }
    }
}