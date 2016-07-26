using System;
using Cactus.Fileserver.Core;

namespace Cactus.Fileserver.Owin.Config
{
    public interface IFileserverConfig
    {
        string Path { get; }

        Func<IFileStorageService> FileStorage { get; }
    }
}