using System;
using Cactus.Fileserver.Core.Model;

namespace Cactus.Fileserver.Core.Storage
{
    public class RandomNameProvider<T> : IStoredNameProvider<T> where T : MetaInfo, new()
    {
        private readonly Random RandomNumberGenerator;
        private readonly byte[] buffer;

        public RandomNameProvider() : this(12)
        {
        }

        public RandomNameProvider(int bytesCount)
        {
            buffer = new byte[bytesCount];
            RandomNumberGenerator = new Random(DateTime.UtcNow.Millisecond);
        }

        public bool StoreExt { get; set; }

        public string GetName(T info)
        {
            RandomNumberGenerator.NextBytes(buffer);
            var res = Convert.ToBase64String(buffer).Replace('+', '-').Replace('/', '_');
            if (info.OriginalName != null && StoreExt)
            {
                var lastDot = info.OriginalName.LastIndexOf('.');
                if (lastDot > 0 && lastDot < info.OriginalName.Length - 1)
                    res += info.OriginalName.Substring(lastDot);
            }

            return res;
        }

        public string Regenerate(T info, string duplicatedName)
        {
            return GetName(info);
        }
    }
}