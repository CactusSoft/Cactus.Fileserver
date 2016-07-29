using Cactus.Fileserver.Owin.Config;
using ImageResizer;

namespace Cactus.Fileserver.Owin.Images.Config
{
    public interface IImageServerConfig : IFileserverConfig
    {
        Instructions DefaultInstructions { get; }

        Instructions MandatoryInstructions { get; }

    }
}
