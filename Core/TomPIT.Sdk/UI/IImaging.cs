using SkiaSharp;

namespace TomPIT.UI
{
    public interface IImaging
    {
        byte[] Resize(SKBitmap image, int maxWidth, int maxHeight, bool padImage);
    }
}
