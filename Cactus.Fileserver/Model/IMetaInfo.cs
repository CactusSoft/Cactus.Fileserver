using System;
using System.Collections.Generic;

namespace Cactus.Fileserver.Model
{
    public interface IMetaInfo
    {
        /// <summary>
        /// URI to get the file
        /// </summary>
        Uri Uri { get; set; }

        /// <summary>
        /// Internal storage URI. Local file system path or S3/Azure storage URI
        /// </summary>
        Uri InternalUri { get; set; }

        /// <summary>
        /// URI of the origin. Used for files that are derivative from other, like resized image
        /// </summary>
        Uri Origin { get; set; }

        /// <summary>
        /// Mime type
        /// </summary>
        string MimeType { get; set; }

        /// <summary>
        /// Original name from user's system
        /// </summary>
        string OriginalName { get; set; }

        /// <summary>
        /// File owner
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// URI of icon of the file
        /// </summary>
        Uri Icon { get; set; }

        /// <summary>
        /// Extra metadata
        /// </summary>
        IDictionary<string, string> Extra { get; set; }
    }
}