namespace Cactus.Fileserver.S3Storage
{
    public interface IS3FileStorageSettings
    {
        string BucketName { get; }
        string Region { get; }
    }

    public interface IS3SecretSettings
    {
        string Region { get; }
        string AccessKey { get; }
        string SecretKey { get; }
    }

    public class S3FileStorageSettings : IS3FileStorageSettings, IS3SecretSettings
    {
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}