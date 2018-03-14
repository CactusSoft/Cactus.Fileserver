using System;
using System.IO;

namespace Cactus.Fileserver.Core.Model
{
    public interface IFileGetContext<T> where T : IFileInfo
    {
        Stream ContextStream { get; set; }

        bool IsNeedToPromoteStream { get; set; }

        Uri RedirectUri { get; set; }

        bool IsNeedToRedirect { get; set; }

        T MetaInfo { get; set; }
    }
}