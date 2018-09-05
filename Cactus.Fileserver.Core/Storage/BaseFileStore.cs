using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public abstract class BaseFileStore : IFileStorage
    {

        public IUriResolver UriResolver { get; protected set; }

        protected BaseFileStore(IUriResolver uriResolver)
        {
            UriResolver = uriResolver;
        }

        public async Task<Uri> Add(Stream stream, IFileInfo info)
        {
            return UriResolver.ResolveUri(await ExecuteAdd(stream, info));
        }

        /// <summary>
        /// Executes actual add, must be overriden
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="info">Result filename</param>
        /// <returns></returns>
        protected abstract Task<string> ExecuteAdd(Stream stream, IFileInfo info);


        public abstract Task Delete(Uri uri);

        public async Task<Stream> Get(Uri uri)
        {
            return await ExecuteGet(UriResolver.ResolveFilename(uri));
        }

        protected abstract Task<Stream> ExecuteGet(string filename);


    }
}