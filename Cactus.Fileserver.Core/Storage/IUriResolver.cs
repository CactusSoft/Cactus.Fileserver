using System;

namespace Cactus.Fileserver.Core.Storage
{
    public interface IUriResolver
    {
        Uri ResolveUri(string newFileName);

        string ResolveFilename(Uri fileUri);

        string ResolvePath(Uri fileUri);

        string ResolvePath(string fileName);


    }
}