using System;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.Aspnet
{
    internal static class HttpExtensions
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

        public static string GetFileName(this HttpContentHeaders headers)
        {
            return headers.ContentDisposition.FileName?.Trim('"');
        }
    }
}