using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;
using Microsoft.Extensions.Logging;

namespace Cactus.Fileserver.LocalStorage
{
    public class LocalFileStorage : IFileStorage
    {
        private const int MaxTriesCount = 10;

        private readonly IStoredNameProvider _nameProvider;
        private readonly IUriResolver _uriResolver;
        private readonly ILogger _log;

        public LocalFileStorage(IStoredNameProvider nameProvider, IUriResolver uriResolver, ILogger<LocalFileStorage> log)
        {
            _nameProvider = nameProvider;
            _uriResolver = uriResolver;
            _log = log;
        }

        public async Task<Uri> Add(Stream stream, IMetaInfo info)
        {
            _log.LogDebug("Adding {filename}", info.OriginalName);
            var filename = _nameProvider.GetName(info);
            var path = _uriResolver.ResolvePath(info);
            var fullFilePath = Path.Combine(path, filename);
            var triesCount = 0;
            for (; File.Exists(fullFilePath) && triesCount < MaxTriesCount; triesCount++)
            {
                await Task.Delay(42); // you know, the answer to the question of life
                filename = _nameProvider.Regenerate(info, filename);
                fullFilePath = Path.Combine(path, filename);
            }

            if (triesCount > MaxTriesCount)
                throw new IOException("Could not generate unique file name");

            using (var dest = new FileStream(fullFilePath, FileMode.CreateNew))
            {
                await stream.CopyToAsync(dest);
            }

            info.InternalUri = new Uri("file://" + fullFilePath);
            info.Uri = _uriResolver.ResolveUri(info);
            _log.LogDebug("{filename} stored as {file}, the full uri is {uri}", info.OriginalName, fullFilePath, info.Uri);
            return info.Uri;
        }

        public Task Delete(IMetaInfo fileInfo)
        {
            _log.LogDebug("Delete file {name}: {uri}", fileInfo.OriginalName, fileInfo.InternalUri.AbsolutePath);
            File.Delete(fileInfo.InternalUri.AbsolutePath);
            return Task.CompletedTask;
        }

        public Task<Stream> Get(IMetaInfo fileInfo)
        {
            _log.LogDebug("Read file {name}: {uri}", fileInfo.OriginalName, fileInfo.InternalUri.AbsolutePath);
            var fullFilePath = fileInfo.InternalUri.AbsolutePath;
            var stream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult((Stream)stream);
        }
    }


}