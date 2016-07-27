using System;
using Cactus.Fileserver.Asp5.Config;
using ImageResizer;

namespace Cactus.Fileserver.Asp5.Images.Config
{
    public class LocalImageServerConfig : LocalFileserverConfig, IImageServerConfig
    {
        public LocalImageServerConfig(string storageFolder, Uri baseUri, string path) : base(storageFolder, baseUri, path)
        {
            //By default restrict image size by 3x2 kpx 
            DefaultInstructions = new Instructions("maxwidth=3000&maxheight=2000&mode=max");
        }

        public Instructions DefaultInstructions { get; set; }
    }
}
