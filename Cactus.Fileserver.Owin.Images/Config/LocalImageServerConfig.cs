using System;
using Cactus.Fileserver.Owin.Config;
using ImageResizer;

namespace Cactus.Fileserver.Owin.Images.Config
{
    public class LocalImageServerConfig : LocalFileserverConfig, IImageServerConfig
    {
        public LocalImageServerConfig(string storageFolder, Uri baseUri, string path) : base(storageFolder, baseUri, path)
        {
            //By default restrict image size by 3x2 kpx 
            DefaultInstructions = new Instructions("autorotate=true&mode=max");
            MandatoryInstructions = new Instructions("maxwidth=3000&maxheight=2000");
        }

        public Instructions DefaultInstructions { get; set; }

        public Instructions MandatoryInstructions { get; set; }
    }
}
