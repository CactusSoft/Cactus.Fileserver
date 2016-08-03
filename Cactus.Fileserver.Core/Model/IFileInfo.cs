using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public interface IFileInfo
    {
        string MimeType { get; set; }

        string Name { get; set; }

        string Owner { get; set; }

        IDictionary<string, string> Extra { get; }
    }
}
