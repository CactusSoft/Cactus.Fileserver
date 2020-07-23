using System.IO;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage.Config
{
    public class LocalMetaStorageOptions
    {
        public LocalMetaStorageOptions()
        {
            Extension = ".json";
        }

        public string BaseFolder { get; set; }
        public string Extension { get; set; }
    }

    public class LocalMetaStorageOptionsValidator : IValidateOptions<LocalMetaStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, LocalMetaStorageOptions options)
        {
            if (options.BaseFolder == null) return ValidateOptionsResult.Fail("BaseFolder cannot be null");
            if (!Directory.Exists(options.BaseFolder)) return ValidateOptionsResult.Fail($"Path {options.BaseFolder} doesn't exist");
            return ValidateOptionsResult.Success;
        }
    }
}
