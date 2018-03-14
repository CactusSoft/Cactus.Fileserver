using System;
using System.IO;
using System.Threading.Tasks;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Cactus.Fileserver.AzureStorage
{
    public class AzureFileStorage<T> : IFileStorage<T> where T : MetaInfo, new()
    {
        private readonly string containerName;
        private readonly IStoredNameProvider<T> nameProvider;
        private readonly string cacheControl;
        private readonly string connectionString;
        private CloudBlobContainer cloudBlobContainer;

        public AzureFileStorage(string connectionString, string containerName, IStoredNameProvider<T> nameProvider)
        {
            this.containerName = containerName;
            this.nameProvider = nameProvider;
            //// sec * min * hour
            cacheControl = $"max-age={60 * 60 * 24}, must-revalidate";
            this.connectionString = connectionString;
            try
            {
                InitStorage().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Init failed {0}: {1}", e.GetType().Name, e.Message);
                //Trace.TraceError("Init failed {0}: {1}", e.GetType().Name, e.Message);
            }
        }

        public IUriResolver UriResolver { get; }

        public async Task<Uri> Add(Stream stream, T info)
        {
            await InitStorage().ConfigureAwait(false);
            var targetFile = nameProvider.GetName(info);
            Console.WriteLine($"Writing {targetFile} azure");
            //Trace.WriteLine($"Writing {targetFile} azure");
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            blockBlob.Properties.ContentType = info.MimeType;
            blockBlob.Properties.CacheControl = cacheControl;

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
            Console.WriteLine($"Delete {targetFile} from azure");
            //Trace.WriteLine($"Delete {targetFile} from azure");
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            await blockBlob.DeleteIfExistsAsync().ConfigureAwait(false);
        }

        public async Task<Stream> Get(Uri uri)
        {
            await InitStorage().ConfigureAwait(false);
            var targetFile = uri.GetResource();
            var blob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            return await blob.OpenReadAsync().ConfigureAwait(false);
        }

        private async Task InitStorage()
        {
            if (cloudBlobContainer == null)
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(connectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    ////Set up DefaultServiceVersion to support Content-Disposition header
                    var serviceProperties = await blobClient.GetServicePropertiesAsync().ConfigureAwait(false);
                    serviceProperties.DefaultServiceVersion = "2013-08-15";
                    await blobClient.SetServicePropertiesAsync(serviceProperties).ConfigureAwait(false);

                    cloudBlobContainer = blobClient.GetContainerReference(containerName);
                    await cloudBlobContainer.CreateIfNotExistsAsync().ConfigureAwait(false);
                    await cloudBlobContainer.SetPermissionsAsync(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        }).ConfigureAwait(false);
                }
                catch
                {
                    cloudBlobContainer = null;
                    throw;
                }
            }
        }
    }
}
