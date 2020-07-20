using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileStorage : IFileStorage
    {
        protected readonly IS3FileStorageSettings Settings;
        protected readonly IAmazonS3 S3Client;
        protected readonly IStoredNameProvider NameProvider;
        protected readonly IUriResolver UriResolver;


        public S3FileStorage(IS3FileStorageSettings settings, IAmazonS3 s3Client, IStoredNameProvider nameProvider, IUriResolver uriResolver)
        {
            Settings = settings;
            S3Client = s3Client;
            NameProvider = nameProvider;
            UriResolver = uriResolver;
        }

        public virtual async Task<Uri> Add(Stream stream, IMetaInfo info)
        {
            var key = NameProvider.GetName(info);
            var putRequest = new PutObjectRequest
            {
                BucketName = Settings.BucketName,
                Key = key,
                InputStream = stream,
                AutoCloseStream = true,
                CannedACL = S3CannedACL.PublicRead
            };
            putRequest.Metadata.Add(nameof(info.OriginalName), info.OriginalName);
            putRequest.Metadata.Add(nameof(info.MimeType), info.MimeType);
            putRequest.Metadata.Add(nameof(info.Owner), info.Owner);

            await S3Client.PutObjectAsync(putRequest);
            return UriResolver.ResolveUri(Settings.Region, Settings.BucketName, key);

        }

        public virtual Task Delete(IMetaInfo fileInfo)
        {
            var key = UriResolver.ResolveKey(fileInfo);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = Settings.BucketName,
                Key = key
            };
            return S3Client.DeleteObjectAsync(deleteRequest);
        }

        public virtual async Task<Stream> Get(IMetaInfo fileInfo)
        {
            var filename = UriResolver.ResolveKey(fileInfo);
            var getRequest = new GetObjectRequest
            {
                BucketName = Settings.BucketName,
                Key = filename
            };

            var response = await S3Client.GetObjectAsync(getRequest);
            return response.ResponseStream;
        }
    }
}