using System.Linq;
using Cactus.Fileserver.ImageResizer.Core.Utils;

namespace Cactus.Fileserver.ImageResizer.Core
{
    public static class InstructionsExtensions
    {
        public static void Join(this Instructions instructions, Instructions join, bool overwrite = false)
        {
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


        public static string GetSizeKey(this Instructions instructions)
        {
            return instructions?.Width != null && instructions.Height != null
                ? instructions.Width + "x" + instructions.Height
                : null;
        }
    }
}
