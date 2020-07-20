using System;
using Cactus.Fileserver.Model;

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

    public class DirectUriResolver : IUriResolver
    {
        public const string AwsUrlFormat = "https://s3.{0}.amazonaws.com/{1}/{2}";

        public Uri ResolveUri(string region, string bucket, string key)
        {
            return new Uri(string.Format(AwsUrlFormat, region, bucket, key));
        }

        public string ResolveKey(IMetaInfo fileInfo)
        {
            return fileInfo.Uri.GetResource();
        }
    }
}