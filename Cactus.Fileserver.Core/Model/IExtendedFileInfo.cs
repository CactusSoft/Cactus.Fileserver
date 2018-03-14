using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public interface IExtendedFileInfo : IFileInfo
    {

        Uri OriginalUri { get; set; }

        bool IsOriginal { get; set; }

        Dictionary<string, Uri> AvaliableSizes { get; }
        
    }
}