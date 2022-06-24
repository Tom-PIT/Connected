using System.Drawing;

namespace TomPIT.UI
{
	public interface IImaging
	{
		byte[] Resize(Image image, int maxWidth, int maxHeight, bool padImage);
	}
}
