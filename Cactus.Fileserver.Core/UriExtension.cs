using System;
using System.IO;

namespace Cactus.Fileserver.Core
{
    public static class UriExtension
    {
        public static string GetResource(this Uri uri)
        {
            return Path.GetFileName(uri.AbsolutePath);
        }
    }
}