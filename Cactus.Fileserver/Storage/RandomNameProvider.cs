using System;
using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.Storage
{
    public class RandomNameProvider : IStoredNameProvider
    {
        private readonly Random _randomNumberGenerator;
        private readonly byte[] _buffer;

        public RandomNameProvider() : this(12)
        {
        }

        public RandomNameProvider(int bytesCount)
        {
            _buffer = new byte[bytesCount];
            _randomNumberGenerator = new Random(DateTime.UtcNow.Millisecond);
        }

        public bool StoreExt { get; set; }

        public string GetName(IFileInfo info)
        {
            _randomNumberGenerator.NextBytes(_buffer);
            var res = Convert.ToBase64String(_buffer).Replace('+', '-').Replace('/', '_');
            if (info.OriginalName != null && StoreExt)
            {
                var lastDot = info.OriginalName.LastIndexOf('.');
                if (lastDot > 0 && lastDot < info.OriginalName.Length - 1)
                    res += info.OriginalName.Substring(lastDot);
            }

            return res;
        }

        public string Regenerate(IFileInfo info, string duplicatedName)
        {
            return GetName(info);
        }
    }
}