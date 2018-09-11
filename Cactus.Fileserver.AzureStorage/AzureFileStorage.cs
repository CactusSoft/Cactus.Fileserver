using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Logging;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Cactus.Fileserver.AzureStorage
{
    public class AzureFileStorage : IFileStorage
    {
        private readonly string _containerName;
        private readonly IStoredNameProvider _nameProvider;
        private readonly string _cacheControl;
        private readonly string _connectionString;
        private CloudBlobContainer _cloudBlobContainer;
        private static readonly ILog Log = LogProvider.GetLogger(typeof(AzureFileStorage));

        public AzureFileStorage(string connectionString, string containerName, IStoredNameProvider nameProvider)
        {
            _containerName = containerName;
            _nameProvider = nameProvider;
            //// sec * min * hour
            _cacheControl = $"max-age={60 * 60 * 24}, must-revalidate";
            _connectionString = connectionString;
            try
            {
                InitStorage().Wait();
            }
            catch (Exception e)
            {
                Log.Error("Init failed {0}: {1}", e.GetType().Name, e.Message);
            }
        }

        public IUriResolver UriResolver { get; }

        public async Task<Uri> Add(Stream stream, IFileInfo info)
        {
            await InitStorage().ConfigureAwait(false);
            var targetFile = _nameProvider.GetName(info);
            Log.Debug("Writing {0} azure", targetFile);
            var blockBlob = _cloudBlobContainer.GetBlockBlobReference(targetFile);
            blockBlob.Properties.ContentType = info.MimeType;
            blockBlob.Properties.CacheControl = _cacheControl;

            if (!info.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                blockBlob.Properties.ContentDisposition =
                    $"attachment;filename=UTF-8''{Uri.EscapeDataString(info.OriginalName)}";
            }

            if (info.Extra != null)
            {
                foreach (var kvp in info.Extra)
                {
                    blockBlob.Metadata.Add(kvp);
                }
            }

            await blockBlob.UploadFromStreamAsync(stream);
            return blockBlob.Uri;
        }

        public async Task Delete(Uri uri)
        {
            await InitStorage().ConfigureAwait(false);
            var targetFile = uri.GetResource();
            Log.Debug("Delete {0} from azure", targetFile);
            var blockBlob = _cloudBlobContainer.GetBlockBlobReference(targetFile);
            await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);
        }

        public async Task<Stream> Get(Uri uri)
        {
            await InitStorage().ConfigureAwait(false);
            var targetFile = uri.GetResource();
            var blob = _cloudBlobContainer.GetBlockBlobReference(targetFile);
            return await blob.OpenReadAsync().ConfigureAwait(false);
        }

        private async Task InitStorage()
        {
            if (_cloudBlobContainer == null)
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(_connectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    ////Set up DefaultServiceVersion to support Content-Disposition header
                    var serviceProperties = await blobClient.GetServicePropertiesAsync().ConfigureAwait(false);
                    serviceProperties.DefaultServiceVersion = "2013-08-15";
                    await blobClient.SetServicePropertiesAsync(serviceProperties).ConfigureAwait(false);

                    _cloudBlobContainer = blobClient.GetContainerReference(_containerName);
                    await _cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
                    await _cloudBlobContainer.SetPermissionsAsync(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        }).ConfigureAwait(false);
                }
                catch
                {
                    _cloudBlobContainer = null;
                    throw;
                }
            }
        }
    }
}
