using System.Collections.Generic;
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

		string Css { get; }

		ListItems<IIoTBinding> Bindings { get; }

		List<IIoTBinding> QueryBindings();
	}
}
