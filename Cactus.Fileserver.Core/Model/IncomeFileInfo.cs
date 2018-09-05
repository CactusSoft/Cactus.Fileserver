using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Core.Model
{
    public class IncomeFileInfo : IFileInfo
    {
        public IncomeFileInfo()
        {
            Extra = new Dictionary<string, string>();
        }

        public IncomeFileInfo(IFileInfo copyFrom) : this()
        {
            if (copyFrom != null)
            {
                Uri = copyFrom.Uri;
                Origin = copyFrom.Origin;
                MimeType = copyFrom.MimeType;
                OriginalName = copyFrom.OriginalName;
                Owner = copyFrom.Owner;
                Icon = copyFrom.Icon;
            }
        }

        public Uri Uri { get; set; }
        public Uri Origin { get; set; }
        public string MimeType { get; set; }
        public string OriginalName { get; set; }
        public string Owner { get; set; }
        public Uri Icon { get; set; }
        public IDictionary<string, string> Extra { get; set; }
    }
}