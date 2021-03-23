using System.Collections.Generic;

namespace TomPIT.Design.Ide.Toolbox
{
	public interface IToolbox
	{
		List<IItemDescriptor> Items { get; }
	}
}
