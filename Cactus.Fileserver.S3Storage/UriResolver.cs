using System;
using Cactus.Fileserver.Model;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.S3Storage
{
    public interface IUriResolver
    {
        /// <summary>
        /// Returns URI to access the file
        /// </summary>
        /// <param name="region"></param>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Uri ResolveUri(string region, string bucket, string key);

        /// <summary>
        /// Returns resource key based on requested url
        /// </summary>
        /// <returns></returns>
        string ResolveKey(IMetaInfo fileInfo);
    }

    public class DirectS3UriResolver : IUriResolver
    {
        public const string AwsUrlFormat = "https://s3.{0}.amazonaws.com/{1}/{2}";

        public Uri ResolveUri(string region, string bucket, string key)
        {
            return new Uri(string.Format(AwsUrlFormat, region, bucket, key));
        }

        public string ResolveKey(IMetaInfo fileInfo)
        {
            return fileInfo.InternalUri.GetResource();
        }
    }

    public class FileserverUriResolver : IUriResolver
    {
        private readonly string _baseUri;

        public FileserverUriResolver(IOptions<S3FileStorageOptions> options)
        {
            _baseUri = options.Value.BaseUri.ToString().TrimEnd('/');
        }

        public Uri ResolveUri(string region, string bucket, string key)
        {
            return new Uri($"{_baseUri}/{bucket}/{key}");
        }

        public string ResolveKey(IMetaInfo fileInfo)
        {
            return fileInfo.InternalUri.GetResource();
        }
    }
}