using System;

namespace Cactus.Fileserver.Core.Model
{
    public class MetaInfo: IFileInfo
    {
        public string MimeType { get; set; }

        public string Name { get; set; }

        public int Size { get; set; }

        public string Owner { get; set; }

        public Uri Uri { get; set; }
    }
}
