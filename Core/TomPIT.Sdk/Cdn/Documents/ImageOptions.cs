using SkiaSharp;

namespace TomPIT.Cdn.Documents
{
    public enum DocumentImageMode
    {
        SingleFile = 0,
        SingleFilePageByPage = 1,
        DifferentFiles = 2
    }

    public enum DocumentTextRenderingMode
    {
        SystemDefault = 0,
        SingleBitPerPixelGridFit = 1,
        SingleBitPerPixel = 2,
        AntiAliasGridFit = 3,
        AntiAlias = 4,
        ClearTypeGridFit = 5
    }

    public class ImageOptions : PageByPageOptions
    {
        public SKColor PageBorderColor { get; set; }
        public int PageBorderWidth { get; set; } = 1;
        public DocumentImageMode Mode { get; set; } = DocumentImageMode.SingleFile;
        public SKEncodedImageFormat Format { get; set; }
        public int Resolution { get; set; } = 96;
        public bool RetainBackgroundTransparency { get; set; }
        public DocumentTextRenderingMode TextRenderingMode { get; set; } = DocumentTextRenderingMode.SystemDefault;
    }
}
