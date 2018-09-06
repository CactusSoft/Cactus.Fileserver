using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Model
{
    /// <summary>
    ///     Represent file info. For example image that you uploaded pic.jpeg and received back URL like
    ///     http://cdn.texas.srv.com/debug-folder/abcdf.png?x=y
    /// </summary>
    public class MetaInfo : IFileInfo
    {
        public MetaInfo()
        {
            Extra = new Dictionary<string, string>();
        }

        public MetaInfo(IFileInfo copyFrom):this()
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

        /// <summary>
        ///     Full URL for getting the file. http://cdn.texas.srv.com/debug-folder/abcdf.png?x=y  in our case
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Origin URI.
        /// </summary>
        public Uri Origin { get; set; }

        /// <summary>
        ///     Server-independent path to the file. In our case "/debug-folder/abcdf.png".
        /// </summary>
        public string StoragePath { get; set; }

        /// <summary>
        ///     The stored file MIME type regarding RFC6838
        ///     In our case "image/png" because the original file was converted into PNG format during uploading
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        ///     The original uploaded file name, "pic.jpg" in our case
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        ///     File owner. Any string that will help you to define the file owner, e-mail for example
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        ///     Icon or thumbnail URI.
        /// </summary>
        public Uri Icon { get; set; }

        /// <summary>
        ///     Any extra parameters. The point for lightweight extensions.
        /// </summary>
        public IDictionary<string, string> Extra { get; set; }
    }
}