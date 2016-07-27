using Cactus.Fileserver.Asp5.Config;
using ImageResizer;

namespace Cactus.Fileserver.Asp5.Images.Config
{
    public interface IImageServerConfig : IFileserverConfig
    {
        Instructions DefaultInstructions { get; }

    }
}
