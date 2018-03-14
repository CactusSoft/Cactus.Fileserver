using System;
using System.IO;

namespace Cactus.Fileserver.Core.Model
{
    public class FileGetContext<T> : IFileGetContext<T> where T : IFileInfo
    {
        public FileGetContext()
        {
        }

        public Stream ContextStream { get; set; }
        public bool IsNeedToPromoteStream { get; set; }
        public Uri RedirectUri { get; set; }
        public bool IsNeedToRedirect { get; set; }
        public T MetaInfo { get; set; }
    }
}