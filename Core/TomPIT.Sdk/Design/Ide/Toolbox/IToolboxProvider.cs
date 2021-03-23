using System.Collections.Generic;

namespace TomPIT.Design.Ide.Toolbox
{
	public interface IToolboxProvider : IEnvironmentObject
	{
		List<IItemDescriptor> Items { get; }
	}
}
