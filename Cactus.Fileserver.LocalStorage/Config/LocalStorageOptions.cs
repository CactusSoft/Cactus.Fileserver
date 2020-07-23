using System;
using System.IO;
using Microsoft.Extensions.Options;

namespace Cactus.Fileserver.LocalStorage.Config
{
    public class LocalFileStorageOptions
    {
        public string BaseFolder { get; set; }
        public Uri BaseUri { get; set; }
    }

    public class LocalFileStorageOptionsValidator : IValidateOptions<LocalFileStorageOptions>
    {
        public ValidateOptionsResult Validate(string name, LocalFileStorageOptions options)
        {
            if (options.BaseUri == null) return ValidateOptionsResult.Fail("BaseUri cannot be null");
            if (options.BaseFolder == null) return ValidateOptionsResult.Fail("BaseFolder cannot be null");
            if (!Directory.Exists(options.BaseFolder)) return ValidateOptionsResult.Fail($"Path {options.BaseFolder} doesn't exist");
            return ValidateOptionsResult.Success;
        }
    }
}
