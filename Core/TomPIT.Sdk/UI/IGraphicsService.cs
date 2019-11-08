using System.Drawing;

namespace TomPIT.UI
{
	public interface IGraphicsService
	{
		byte[] CreateImage(string text, int width, int height);
		byte[] CreateImage(string text, int width, int height, Color color);
		byte[] Resize(Image image, int maxWidth, int maxHeight, bool padImage);
	}
}
