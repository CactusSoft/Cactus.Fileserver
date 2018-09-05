using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Cactus.Fileserver.Core.Model;
using Microsoft.AspNetCore.Http;

[assembly: InternalsVisibleTo("Cactus.Fileserver.Tests")]
namespace Cactus.Fileserver.Core
{
    public static class HttpContextExtensions
    {
        public static FileContext GetNewFileContext(this HttpContext context) 
        {
            return ExtractFileContext(context.Items);
        }

        public static void PutNewFileContext(this HttpContext context, FileContext fileContext)
        { 
            AddOrUpdateFileContext(context.Items, fileContext);
        }

        internal static void AddOrUpdateFileContext(IDictionary<object, object> items, FileContext fileContext) 
        {
            items["cactus.fileserver.context"] = fileContext;
        }

        internal static FileContext ExtractFileContext(IDictionary<object, object> items)
        {
            if (items.TryGetValue("cactus.fileserver.context", out var val))
            {
                return val as FileContext;
            }

            return null;
        }
    }

    public class FileContext
    {
        public IFileInfo IncomeFileInfo { get; set; }

        public IFileInfo MetaFileInfo { get; set; }

        public HttpContent HttpContent { get; set; }
    }
}
