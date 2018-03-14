using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cactus.Fileserver.Core;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileStorage<T> : BaseFileStore<T> where T : IFileInfo
    {
        public const string AwsUrlFormat = "https://s3.{0}.amazonaws.com/{1}/{2}";
        public const string AwsUrlReplace = "https://s3.{0}.amazonaws.com/{1}/";
        private readonly string _bucketName;
        private readonly IStoredNameProvider<T> _nameProvider;
        private readonly Func<AmazonS3Client> _amazonClientFactory;


        public S3FileStorage(string bucketName, RegionEndpoint regionEndpoint, string accessKey, string secretKey, Uri fileserverBaseUri, IStoredNameProvider<T> nameProvider) : base(new DefaultUriResolver(bucketName, regionEndpoint, fileserverBaseUri))
        {
            _bucketName = bucketName;
            _nameProvider = nameProvider;
            _amazonClientFactory = () => new AmazonS3Client(accessKey, secretKey, regionEndpoint);
        }

        public S3FileStorage(string bucketName, Uri fileserverBaseUri, IStoredNameProvider<T> nameProvider) : base(new DefaultUriResolver(bucketName, new AmazonS3Client().Config.RegionEndpoint, fileserverBaseUri))
        {
            _bucketName = bucketName;
            _nameProvider = nameProvider;
            _amazonClientFactory = () => new AmazonS3Client();
        }

        protected override async Task<string> ExecuteAdd(Stream stream, T info)
        {
            var key = _nameProvider.GetName(info);
            using (var client = _amazonClientFactory.Invoke())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    AutoCloseStream = true,
                    CannedACL = S3CannedACL.PublicRead
                };
                putRequest.Metadata.Add(nameof(info.OriginalName), info.OriginalName);
                putRequest.Metadata.Add(nameof(info.MimeType), info.MimeType);
                putRequest.Metadata.Add(nameof(info.Owner), info.Owner);

                await client.PutObjectAsync(putRequest);
                return key;
            }
        }

        public override async Task Delete(Uri uri)
        {
            var key = uri.Segments[1];
            using (var client = _amazonClientFactory.Invoke())
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await client.DeleteObjectAsync(deleteRequest);
            }
        }

        protected override async Task<Stream> ExecuteGet(string filename)
        {
            using (var client = _amazonClientFactory.Invoke())
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filename
                };

                var responce = await client.GetObjectAsync(getRequest);
                return responce.ResponseStream;
            }
        }


        protected class DefaultUriResolver : IUriResolver
        {

            private readonly string _bucketName;
            private readonly RegionEndpoint _regionEndpoint;
            private readonly Uri _fileserverBaseUri;

            public DefaultUriResolver(string bucketName, RegionEndpoint regionEndpoint, Uri fileserverBaseUri)
            {
                _bucketName = bucketName;
                _regionEndpoint = regionEndpoint;
                _fileserverBaseUri = fileserverBaseUri;
            }

            public Uri ResolveStaticUri(Uri currentUri)
            {
                return new Uri(string.Format(AwsUrlFormat, _regionEndpoint.SystemName, _bucketName, ResolveFilename(currentUri)));;
            }

            public Uri ResolveUri(string newFileName)
            {
                return new Uri(_fileserverBaseUri, newFileName);
            }

            public string ResolveFilename(Uri fileUri)
            {
                return fileUri.GetResource();
            }

            public string ResolvePath(Uri fileUri)
            {
                throw new NotImplementedException("Not implemented for this store");
            }

            public string ResolvePath(string fileName)
            {
                throw new NotImplementedException("Not implemented for this store");
            }
        }
    }
}