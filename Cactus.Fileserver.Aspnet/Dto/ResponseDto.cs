using System;
using System.Collections.Generic;
using System.Linq;
using Cactus.Fileserver.Model;

namespace Cactus.Fileserver.Aspnet.Dto
{
    public class ResponseDto
    {

        public ResponseDto() { }

        public ResponseDto(IMetaInfo meta)
        {
            Uri = meta.Uri;
            Extra = meta.Extra?.ToDictionary(e => e.Key, e => e.Value);
            Owner = meta.Owner;
            Origin = meta.Origin;
            OriginalName = meta.OriginalName;
            MimeType = meta.MimeType;
            Icon = meta.Icon;
        }
        /// <summary>
        /// Not null if a error took place
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        ///     Full URL for getting the file. http://cdn.texas.srv.com/debug-folder/abcdf.png?x=y  in our case
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Origin URI.
        /// </summary>
        public Uri Origin { get; set; }

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
