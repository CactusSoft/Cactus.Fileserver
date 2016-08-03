using System;
using System.Diagnostics;
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
                InitStorage();
            }
            catch (Exception e)
            {
                Trace.TraceError("Init failed {0}: {1}", e.GetType().Name, e.Message);
            }
        }

        public async Task<Uri> Add(Stream stream, T info)
        {
            InitStorage();
            var targetFile = nameProvider.GetName(info);
            Trace.WriteLine($"Writing {targetFile} azure");
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            blockBlob.Properties.ContentType = info.MimeType;
            blockBlob.Properties.CacheControl = cacheControl;

            if (!info.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                blockBlob.Properties.ContentDisposition =
                    $"attachment;filename=UTF-8''{Uri.EscapeDataString(info.Name)}";
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
            InitStorage();
            var targetFile = uri.GetResource();
            Trace.WriteLine($"Delete {targetFile} from azure");
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            await blockBlob.DeleteIfExistsAsync();
        }

        public async Task<Stream> Get(Uri uri)
        {
            InitStorage();
            var targetFile = uri.GetResource();
            var blob = cloudBlobContainer.GetBlockBlobReference(targetFile);
            return await blob.OpenReadAsync();
        }

        private void InitStorage()
        {
            if (cloudBlobContainer == null)
            {
                try
                {
                    var storageAccount = CloudStorageAccount.Parse(connectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    ////Set up DefaultServiceVersion to support Content-Disposition header
                    var serviceProperties = blobClient.GetServiceProperties();
                    serviceProperties.DefaultServiceVersion = "2013-08-15";
                    blobClient.SetServiceProperties(serviceProperties);

                    cloudBlobContainer = blobClient.GetContainerReference(containerName);
                    cloudBlobContainer.CreateIfNotExists();
                    cloudBlobContainer.SetPermissions(
                        new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Blob
                        });
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
