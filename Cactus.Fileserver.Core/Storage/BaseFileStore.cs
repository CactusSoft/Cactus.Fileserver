using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public abstract class BaseFileStore<T> : IFileStorage<T> where T : MetaInfo, new()
    {

        protected readonly IUriResolver UriResolver;

        protected BaseFileStore(IUriResolver uriResolver)
        {
            UriResolver = uriResolver;
        }

        public async Task<Uri> Add(Stream stream, T info)
        {
            return UriResolver.ResolveUri(await ExecuteAdd(stream, info));
        }

        protected abstract Task<string> ExecuteAdd(Stream stream, T info);


        public abstract Task Delete(Uri uri);

        public async Task<Stream> Get(Uri uri)
        {
            return await ExecuteGet(UriResolver.ResolveFilename(uri));
        }

        protected abstract Task<Stream> ExecuteGet(string filename);


    }
}