using System;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.AspNetCore
{
    public static class HttpRequestExtension
    {
        public static Uri GetAbsoluteUri(this HttpRequest request)
        {
            return new Uri(string.Concat(
                        request.Scheme,
                        "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        request.Path.ToUriComponent(),
                        request.QueryString.ToUriComponent()));
        }
    }
}
