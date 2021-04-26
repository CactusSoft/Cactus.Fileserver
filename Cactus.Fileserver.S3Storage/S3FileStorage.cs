using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Cactus.Fileserver.Model;
using Cactus.Fileserver.Storage;

namespace Cactus.Fileserver.S3Storage
{
    public class S3FileStorage : IFileStorage
    {
        protected readonly IS3FileStorageOptions Settings;
        protected readonly IAmazonS3 S3Client;
        protected readonly IStoredNameProvider NameProvider;
        protected readonly IUriResolver UriResolver;


        public S3FileStorage(IS3FileStorageOptions settings, IAmazonS3 s3Client, IStoredNameProvider nameProvider, IUriResolver uriResolver)
        {
            Settings = settings;
            S3Client = s3Client;
            NameProvider = nameProvider;
            UriResolver = uriResolver;
        }

        public virtual async Task<Uri> Add(Stream stream, IMetaInfo info)
        {
            var streamToUpload = stream;
            var disposeRequired = false;
            if (!stream.CanSeek)
            {
                //AWS requires stream to have defined Length (be seakable)
                //Here the income stream could be 'raw from request' with unknown length
                //So, let's read it into MemoryStream as a buffer
                streamToUpload = new MemoryStream();
                await stream.CopyToAsync(streamToUpload);
                streamToUpload.Seek(0, 0);
                disposeRequired = true;
            }

            try
            {
                var key = NameProvider.GetName(info);
                var putRequest = new PutObjectRequest
                {
                    BucketName = Settings.BucketName,
                    Key = key,
                    InputStream = streamToUpload,
                    AutoCloseStream = true,
                    CannedACL = S3CannedACL.PublicRead
                };
                var asciiOriginName = new string(Encoding.ASCII.GetChars(Encoding.ASCII.GetBytes(info.OriginalName))
                    .Select(c => c == '?' ? '-' : c)
                    .ToArray());
                putRequest.Metadata.Add(nameof(info.OriginalName), asciiOriginName);
                putRequest.Metadata.Add(nameof(info.MimeType), info.MimeType);
                putRequest.Metadata.Add(nameof(info.Owner), info.Owner);

                var res = await S3Client.PutObjectAsync(putRequest);
                info.InternalUri = new Uri($"https://s3.{Settings.Region}.amazonaws.com/{Settings.BucketName}/{key}");
                info.Uri = UriResolver.ResolveUri(Settings.Region, Settings.BucketName, key);
                return info.Uri;
            }
            finally
            {
                if (disposeRequired)
                    streamToUpload.Dispose();
            }
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