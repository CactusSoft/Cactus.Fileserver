using System;
using System.IO;

namespace Cactus.Fileserver
{
    public static class UriExtension
    {
        public static string GetResource(this Uri uri)
        {
            return Path.GetFileName(uri.AbsolutePath);
        }
    }
}