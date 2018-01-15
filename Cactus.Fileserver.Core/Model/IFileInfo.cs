using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public interface IFileInfo
    {
        string MimeType { get; set; }

        string OriginalName { get; set; }

        string Owner { get; set; }

        Uri Icon { get; set; }

        IDictionary<string, string> Extra { get; }
    }
}