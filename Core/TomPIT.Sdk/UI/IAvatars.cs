using System.Drawing;

namespace TomPIT.UI
{
	public interface IAvatars
	{
		byte[] Create(string text, int width, int height);
		byte[] Create(string text, int width, int height, Color color);
	
	}
}
