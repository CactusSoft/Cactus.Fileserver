using System;
using Cactus.Fileserver.Core;

namespace Cactus.Fileserver.Asp5.Config
{
    public interface IFileserverConfig
    {
        string Path { get; }

        Func<IFileStorageService> FileStorage { get; }
    }
}