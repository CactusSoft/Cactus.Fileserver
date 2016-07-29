using System.Linq;
using ImageResizer;

namespace Cactus.Fileserver.Owin.Images
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
    }
}
