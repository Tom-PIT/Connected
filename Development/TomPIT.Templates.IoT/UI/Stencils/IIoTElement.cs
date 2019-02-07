using TomPIT.ComponentModel;

namespace TomPIT.IoT.UI.Stencils
{
	public interface IIoTElement : IElement
	{
		string Name { get; }
		int Left { get; }
		int Top { get; }
		int Width { get; }
		int Height { get; }
	}
}
