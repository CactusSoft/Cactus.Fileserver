using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public class MetaInfo: IFileInfo
    {
        public string MimeType { get; set; }

        public string Name { get; set; }

        public string Owner { get; set; }

        public Uri Uri { get; set; }

        public IDictionary<string,string> Extra { get; set; } 
    }
}
