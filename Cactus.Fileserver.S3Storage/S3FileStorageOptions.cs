namespace Cactus.Fileserver.S3Storage
{
    public interface IS3FileStorageOptions
    {
        string BucketName { get; }
        string Region { get; }
    }

    public interface IS3SecretOptions
    {
        string Region { get; }
        string AccessKey { get; }
        string SecretKey { get; }
    }

    public class S3FileStorageOptions : IS3FileStorageOptions, IS3SecretOptions
    {
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}