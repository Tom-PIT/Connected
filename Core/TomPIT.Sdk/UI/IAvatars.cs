using SkiaSharp;

namespace TomPIT.UI
{
    public interface IAvatars
    {
        byte[] Create(string text, int width, int height);
        byte[] Create(string text, int width, int height, SKColor color);

    }
}
