using System.Linq;
using Cactus.Fileserver.ImageResizer.Utils;

namespace Cactus.Fileserver.ImageResizer
{
    internal static class InstructionsExtensions
    {
        internal static void Join(this Instructions instructions, Instructions join, bool overwrite = false)
        {
            if (join == null) return;
            foreach (var key in join.AllKeys)
            {
                var hasKey = instructions.AllKeys.Any(k => k == key);
                if (overwrite && hasKey)
                {
                    instructions.Remove(key);
                    instructions.Add(key, join[key]);
                }
                else if (!hasKey)
                {
                    instructions.Add(key, join[key]);
                }
            }
        }

        internal static string GetSizeKey(this Instructions instructions)
        {
            if (instructions == null)
                return null;
            var width = instructions.Width.HasValue ? instructions.Width.ToString() : "NA";
            var height = instructions.Height.HasValue ? instructions.Height.ToString() : "NA";
            return "alt-size-" + width + "x" + height;
        }
    }
}
