using System;
using System.IO;
using System.Linq;

namespace Cactus.Fileserver
{
    public static class UriExtension
    {
        public static readonly char UriPathSeparator = '/';
        public static string GetResource(this Uri uri)
        {
            return Path.GetFileName(uri.AbsolutePath.TrimStart(UriPathSeparator));
        }

        public static Uri GetFolder(this Uri uri)
        {
            if (uri == null) return null;
            if (uri.AbsolutePath.Length == 1 && uri.AbsolutePath[0] == UriPathSeparator) return uri;
            var str = uri.ToString();
            return str.Last() == UriPathSeparator ? uri : new Uri(str.Substring(0, str.LastIndexOf(UriPathSeparator)));
        }
    }
}