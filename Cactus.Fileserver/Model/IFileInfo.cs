using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Model
{
    public interface IFileInfo
    {
        Uri Uri { get; set; }

        Uri Origin { get; set; }
        
        string MimeType { get; set; }

        string OriginalName { get; set; }

        string Owner { get; set; }

        Uri Icon { get; set; }

        IDictionary<string, string> Extra { get; set; }
    }
}