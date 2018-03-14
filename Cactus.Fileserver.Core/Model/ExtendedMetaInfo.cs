using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public class ExtendedMetaInfo : MetaInfo, IExtendedFileInfo
    {
        public ExtendedMetaInfo()
        {
            AvaliableSizes = new Dictionary<string, Uri>();
        }

        public ExtendedMetaInfo(ExtendedMetaInfo metaInfo) : base(metaInfo)
        {
            IsOriginal = false;
            OriginalUri = metaInfo.Uri;
        }

        public Uri OriginalUri { get; set; }
        public bool IsOriginal { get; set; }
        public Dictionary<string, Uri> AvaliableSizes { get; }
    }
}