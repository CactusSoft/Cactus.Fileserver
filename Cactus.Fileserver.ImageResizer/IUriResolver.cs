using System;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.ImageResizer
{
    public interface IUriResolver
    {
        Uri Resolve(HttpRequest request);
    }

    public class DefaultUriResolver : IUriResolver
    {
        public Uri Resolve(HttpRequest request)
        {
            return new Uri(string.Concat(
                request.Scheme,
                "://",
                request.Host.ToUriComponent(),
                request.PathBase.ToUriComponent(),
                request.Path.ToUriComponent()));
        }
    }
}