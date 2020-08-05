using System;
using System.IO;
using System.Linq;

namespace Cactus.Fileserver
{
    public static class UriExtension
    {
        public static string GetResource(this Uri uri)
        {
            return Path.GetFileName(uri.AbsolutePath.TrimStart('/'));
        }

        public static Uri GetFolder(this Uri uri)
        {
            if (uri == null) return null;
            if (uri.AbsolutePath == "/") return uri;
            var str = uri.ToString();
            if (str.Last() == '/') return uri;
            return new Uri(str.Substring(0, str.LastIndexOf('/')));
        }
    }
}