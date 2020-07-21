using System;
using Microsoft.AspNetCore.Http;

namespace Cactus.Fileserver.ImageResizer.Utils
{
    public class ResizeInstructions
    {
        public ResizeInstructions()
        {
            MaxWidth = 4096;
            MaxHeight = 4096; //4k by any dimension
        }

        public ResizeInstructions(QueryString queryString)
        {
            if (!queryString.HasValue)
                return;
            var qString = queryString.Value;
            if (qString[0] == '?' && qString.Length > 1)
                qString = qString.Substring(1);

            foreach (var param in qString.Split('&'))
            {
                var kvp = param.Split('=');
                if (kvp.Length == 2)
                {
                    if (nameof(Width).Equals(kvp[0], StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(kvp[1], out var v) && v > 0)
                            Width = v;
                    }
                    else if (nameof(Height).Equals(kvp[0], StringComparison.OrdinalIgnoreCase))
                    {
                        if (int.TryParse(kvp[1], out var v) && v > 0)
                            Height = v;
                    }
                    else if (nameof(KeepAspectRatio).Equals(kvp[0], StringComparison.OrdinalIgnoreCase))
                    {
                        if (bool.TryParse(kvp[1], out var v))
                            KeepAspectRatio = v;
                    }
                }
            }
        }

        public int? Width { get; set; }
        public int? Height { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public bool? KeepAspectRatio { get; set; }

        public void Join(ResizeInstructions instructions, bool @override = false)
        {
            if (instructions == null) return;
            if (instructions.Width.HasValue && (!Width.HasValue || @override))
                Width = instructions.Width;

            if (instructions.Height.HasValue && (!Height.HasValue || @override))
                Height = instructions.Height;

            if (instructions.KeepAspectRatio.HasValue && (!KeepAspectRatio.HasValue || @override))
                KeepAspectRatio = instructions.KeepAspectRatio;

            if (@override)
            {
                MaxWidth = instructions.MaxWidth;
                MaxHeight = instructions.MaxHeight;
            }
        }
    }

    public static class ResizeInstructionsExtensions
    {
        public static string BuildSizeKey(this ResizeInstructions instructions)
        {
            return "alt_size_"
                   + (instructions.Width?.ToString() ?? "-")
                   + 'x'
                   + (instructions.Height?.ToString() ?? "-");
        }
    }
}