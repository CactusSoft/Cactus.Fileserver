using System.Collections.Specialized;
using System.Web;
using ImageSharp.Processing;

namespace Cactus.Fileserver.ImageResizer.Core.Utils
{
    public class Instructions : QuerystringBase<Instructions>
    {
        /// <summary>Creates an empty instructions collection.</summary>
        public Instructions()
        {
        }

        /// <summary>
        /// Copies the specified collection into a new Instructions instance.
        /// </summary>
        /// <param name="col"></param>
        public Instructions(NameValueCollection col)
          : base(col)
        {
        }

        /// <summary>
        /// Copies the specified collection into a new Instructions instance.
        /// </summary>
        /// <param name="queryString">HTTP-like query string</param>
        public Instructions(string queryString)
            : base(HttpUtility.ParseQueryString(queryString))
        {
        }

        /// <summary>
        /// The width in pixels to constrain the image to. See 'Mode' and 'Scale' for constraint logic.
        /// </summary>
        public int? Width
        {
            get => this.Get<int>("width", this.Get<int>("w"));
            set
            {
                this.Set<int>("width", value);
                this.Remove("w");
            }
        }

        /// <summary>
        /// The height in pixels to constrain the image to. See 'Mode' and 'Scale' for constraint logic.
        /// </summary>
        public int? Height
        {
            get => this.Get<int>("height", this.Get<int>("h"));
            set
            {
                this.Set<int>("height", value);
                this.Remove("h");
            }
        }

        /// <summary>
        /// The fit mode to use when both Width and Height are specified. Defaults to Pad.
        /// </summary>
        public ResizeMode? Mode
        {
            get
            {
                return this.Get<ResizeMode>("mode");
            }
            set
            {
                this.Set<ResizeMode>("mode", value);
            }
        }


        /// <summary>The frame of the animated GIF to display. 1-based</summary>
        public int? Frame
        {
            get => this.Get<int>("frame");
            set => this.Set<int>("frame", value);
        }

        /// <summary>The page of the TIFF file to display. 1-based</summary>
        public int? Page
        {
            get => this.Get<int>("page");
            set => this.Set<int>("page", value);
        }

        /// <summary>
        /// Determines Jpeg encoding quality. Maps to 'quality' setting.
        /// </summary>
        public int? JpegQuality
        {
            get => this.Get<int>("quality");
            set => this.Set<int>("quality", value);
        }

        /// <summary>
        /// Maps to 'colors'. Sets the palette size for the final png or gif image (not relevant for jpegs).
        /// Set to 'null' to use the largest palette size available in the format.
        /// Requires the PrettyGifs or WicEncoder plugin.
        /// </summary>
        public byte? PaletteSize
        {
            get => this.Get<byte>("colors");
            set => this.Set<byte>("colors", value);
        }

        /// <summary>
        /// A multiplier to apply to all sizing settings (still obeys Scale=down, though). Useful when you need to apply a page-wide scaling factor, such as for mobile devices.
        /// </summary>
        public double? Zoom
        {
            get => this.Get<double>("zoom");
            set => this.Set<double>("zoom", value);
        }

        /// <summary>
        /// Defines the horizontal width of the crop rectangle's coordinate space. For example, setting this to 100 makes the crop X1 and X2 values percentages of the image width.
        /// </summary>
        public double? CropXUnits
        {
            get => this.Get<double>("cropxunits");
            set => this.Set<double>("cropxunits", value);
        }

        /// <summary>
        /// Defines the vertical height of the crop rectangle's coordinate space. For example, setting this to 100 makes the crop Y1 and Y1 values percentages of the image height.
        /// </summary>
        public double? CropYUnits
        {
            get => this.Get<double>("cropyunits");
            set => this.Set<double>("cropyunits", value);
        }

        /// <summary>
        /// An X1,Y1,X2,Y2 array of coordinates. Unless CropXUnits and CropYUnits are specified, these are in the coordinate space of the original image.
        /// </summary>
        public double[] CropRectangle
        {
            get => this.GetList<double>("crop", new double?(0.0), 4);
            set => this.SetList<double>("crop", value, true, 4);
        }

        /// <summary>
        /// Automatically rotates images based on gravity sensor data embedded in Exif. Requires the AutoRotate plugin
        /// </summary>
        public bool? AutoRotate
        {
            get => this.Get<bool>("autorotate");
            set => this.Set<bool>("autorotate", value);
        }

        /// <summary>
        /// Maps to 'srotate'. Rotates the source image prior to processing. Only 90 degree angles are currently supported.
        /// </summary>
        public double? SourceRotate
        {
            get => this.Get<double>("srotate");
            set => this.Set<double>("srotate", value);
        }

        /// <summary>
        /// Maps to 'rotate'. Rotates the image during rendering. Arbitrary angles are supported.
        /// </summary>
        public double? Rotate
        {
            get => this.Get<double>("rotate");
            set => this.Set<double>("rotate", value);
        }

        /// <summary>
        /// Use 'OutputFormat' unless you need a custom value. Determines the format and encoding of the output image.
        /// </summary>
        public string Format
        {
            get => !string.IsNullOrEmpty(this["format"]) ? this["format"] : this["thumbnail"];
            set
            {
                this["format"] = value;
                this.Remove("thumbnail");
            }
        }


        /// <summary>
        /// If true, the ICC profile will be discared instead of being evaluated server side (which typically causes inconsistent and unexpected effects).
        /// </summary>
        public bool? IgnoreICC
        {
            get => this.Get<bool>("ignoreicc");
            set => this.Set<bool>("ignoreicc", value);
        }

        /// <summary>
        /// The fallback image to redirect to if the original image doesn't exist. Must be the name of a pre-defined 404 image or a filename in the default 404 images directory.
        /// Requires the Image404 plugin to be installed.
        /// </summary>
        public string FallbackImage
        {
            get => this["404"];
            set => this["404"] = value;
        }

        /// <summary>
        /// The color of margin and padding regions. Defaults to Transparent, or White (when jpeg is the selected output color).
        /// </summary>
        public string BackgroundColor
        {
            get => this["bgcolor"];
            set => this["bgcolor"] = value;
        }

        /// <summary>
        /// Defaults to 'bgcolor'. Allows a separate color to be used for padding areas vs. margins.
        /// </summary>
        public string PaddingColor
        {
            get => this["paddingcolor"];
            set => this["paddingcolor"] = value;
        }

        /// <summary>
        /// The color to draw the border with, if a border width is specified.
        /// </summary>
        public string BorderColor
        {
            get => this["bordercolor"];
            set => this["bordercolor"] = value;
        }

        /// <summary>
        /// The name of a pre-defined preset, or a comma-delimited list of presets to apply. These may overwrite other settings. Requires the Presets plugin.
        /// </summary>
        public string Preset
        {
            get => this["preset"];
            set => this["preset"] = value;
        }

        /// <summary>
        /// The name of a pre-defined watermark layer or group from Web.config, or a comma-delimited list of names. Requires the Watermark plugin.
        /// </summary>
        public string Watermark
        {
            get => this["watermark"];
            set => this["watermark"] = value;
        }

        /// <summary>
        /// Applies a Negative filter to the image. Requires the SimpleFilters plugin
        /// </summary>
        public bool? Invert
        {
            get => this.Get<bool>("s.invert");
            set => this.Set<bool>("s.invert", value);
        }

        /// <summary>
        /// Applies a Sepia filter to the image. Requires the SimpleFilters plugin
        /// </summary>
        public bool? Sepia
        {
            get => this.Get<bool>("s.sepia");
            set => this.Set<bool>("s.sepia", value);
        }

        /// <summary>
        /// Applies the specified kind of grayscale filter to the image. Requires the SimpleFilters plugin
        /// </summary>
        public GrayscaleMode? Grayscale
        {
            get => this.Get<GrayscaleMode>("s.grayscale");
            set => this.Set<GrayscaleMode>("s.grayscale", value);
        }

        /// <summary>
        /// Value between 0 and 1. Makes the rendered image transparent. Does not affect borders or background colors - those accept 4-byte colors with alpha channels, however.
        /// Requires the SimpleFilters plugin. Unless the output format is PNG, the image will be blended against white or the background color.
        /// </summary>
        public double? Alpha
        {
            get => this.Get<double>("s.alpha");
            set => this.Set<double>("s.alpha", value);
        }

        /// <summary>
        /// -1..1 Adjust the brightness of the image. Requires the SimpleFilters plugin
        /// </summary>
        public double? Brightness
        {
            get => this.Get<double>("s.brightness");
            set => this.Set<double>("s.brightness", value);
        }

        /// <summary>
        /// -1..1 Adjust the contrast of the image. Requires the SimpleFilters plugin
        /// </summary>
        public double? Contrast
        {
            get => this.Get<double>("s.contrast");
            set => this.Set<double>("s.contrast", value);
        }

        /// <summary>
        /// -1..1 Adjust the saturation of the image. Requires the SimpleFilters plugin
        /// </summary>
        public double? Saturation
        {
            get => this.Get<double>("s.saturation");
            set => this.Set<double>("s.saturation", value);
        }

        /// <summary>
        /// Setting this enables automatic whitespace trimming using an energy function. 50 is safe, even 255 rarely cuts anything off except a shadow. Set TrimPadding to pad the result slightly and improve appearance.
        /// Requires the WhitespaceTrimmer plugin.
        /// </summary>
        public byte? TrimThreshold
        {
            get => this.Get<byte>("trim.threshold");
            set => this.Set<byte>("trim.threshold", value);
        }

        /// <summary>
        /// Set TrimThreshold first. This specifies a percentage of the image size to 'add' to the crop rectangle. Setting to 0.5 or 1 usually produces good results.
        /// Requires the WhitespaceTrimmer plugin.
        /// </summary>
        public double? TrimPadding
        {
            get => this.Get<double>("trim.percentpadding");
            set => this.Set<double>("trim.percentpadding", value);
        }

        /// <summary>Guassian Blur. Requires the AdvancedFilters plugin.</summary>
        public double? Blur
        {
            get => this.Get<double>("a.blur");
            set => this.Set<double>("a.blur", value);
        }

        /// <summary>Unsharp Mask. Requires the AdvancedFilters plugin.</summary>
        public double? Sharpen
        {
            get => this.Get<double>("a.sharpen");
            set => this.Set<double>("a.sharpen", value);
        }

        /// <summary>
        /// Safe noise removal. Requires the AdvancedFilters plugin.
        /// </summary>
        public double? RemoveNoise
        {
            get => this.Get<double>("a.removenoise");
            set => this.Set<double>("a.removenoise", value);
        }

        /// <summary>
        /// Controls dithering when rendering to an 8-bit PNG or GIF image. Requires PrettyGifs or WicEncoder. Accepted values for PrettyGifs: true|false|4pass|30|50|79|[percentage]. Accepted values for WicEncoder: true|false.
        /// </summary>
        public string Dither
        {
            get => this["dither"];
            set => this["dither"] = value;
        }

        /// <summary>
        /// Specify a preferred encoder for compressing the output image file. Defaults to 'gdi'. Other valid values are 'freeimage' and 'wic', which require the FreeImageEncoder and WicEncoder plugins respectively.
        /// FreeImage offers faster jpeg encoding, while WIC offers faster PNG and GIF encoding. Both, however, require full trust.
        /// </summary>
        public string Encoder
        {
            get => this["encoder"];
            set => this["encoder"] = value;
        }

        /// <summary>
        /// Specify a preferred decoder for parsing the original image file. Defaults to 'gdi'. Other values include 'freeimage', 'wic', and 'psdreader'. The preferred decoder gets the first chance at reading the files. If that fails, all other decoders try, in order of declaration in Web.config.
        /// Requires the matching FreeImageDecoder, WicDecoder, or PsdReader plugin to be installed.
        /// </summary>
        public string Decoder
        {
            get => this["decoder"];
            set => this["decoder"] = value;
        }

        /// <summary>
        /// Specify the image processing pipeline to use. Defaults to 'gdi'. If FreeImageBuilder or WicBuilder is installed, you can specify 'freeimage' or 'wic' to use that pipeline instead.
        /// The WIC pipeline offers a 2-8X performance increase of GDI, at the expense of slightly reduced image quality, the full trust requirement, and support for only basic resize and crop commands.
        /// FreeImage offers *nix-level image support, and handles many images that gdi and wic can't deal with. It is also restricted to a subset of the full command series.
        /// </summary>
        public string Builder
        {
            get => this["builder"];
            set => this["builder"] = value;
        }

        /// <summary>
        /// Gets or sets a 1 or 4-element array defining cornder radii. If the array is 1 element, it applies to all corners. If it is 4 elements, each corner gets an individual radius. Values are percentages of the image width or height, whichever is smaller.
        /// Requires the SimpleFilters plugin.
        /// </summary>
        public double[] RoundCorners
        {
            get => this.GetList<double>("s.roundcorners", new double?(0.0), 4, 1);
            set => this.SetList<double>("s.roundcorners", value, true, 4, 1);
        }
    }
}