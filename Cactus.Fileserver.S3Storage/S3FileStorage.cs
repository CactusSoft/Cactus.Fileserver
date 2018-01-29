using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Cactus.Fileserver.Core.Model;
using Cactus.Fileserver.Core.Storage;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileStorage<T> : IFileStorage<T> where T : MetaInfo, new()
    {
        private const string _awsUrlFormat = "https://s3.{0}.amazonaws.com/{1}/{2}";
        private const string _awsUrlReplace = "https://s3.{0}.amazonaws.com/{1}/";
        private readonly string _accessKey;
        private readonly string _bucketName;
        private readonly IStoredNameProvider<T> _nameProvider;
        private readonly string _secretKey;


        public S3FileStorage(string bucketName, string accessKey, string secretKey, IStoredNameProvider<T> nameProvider)
        {
            _bucketName = bucketName;
            _accessKey = accessKey;
            _secretKey = secretKey;
            _nameProvider = nameProvider;
        }

        public async Task<Uri> Add(Stream stream, T info)
        {
            var key = _nameProvider.GetName(info);
            using (var client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.EUCentral1))
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
                return new Uri(string.Format(_awsUrlFormat, RegionEndpoint.EUCentral1.SystemName, _bucketName, key));
            }
        }

        public async Task Delete(Uri uri)
        {
            var key = uri.Segments[1];
            using (var client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.EUCentral1))
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await client.DeleteObjectAsync(deleteRequest);
            }
        }

        public async Task<Stream> Get(Uri uri)
        {
            var key = uri.AbsoluteUri.Replace(
                string.Format(_awsUrlReplace, RegionEndpoint.EUCentral1.SystemName, _bucketName), "");
            using (var client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.EUCentral1))
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var responce = await client.GetObjectAsync(getRequest);
                return responce.ResponseStream;
            }
        }
    }
}