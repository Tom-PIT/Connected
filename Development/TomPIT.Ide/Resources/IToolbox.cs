using System.Collections.Generic;
using TomPIT.Ide.Collections;

namespace TomPIT.Ide.Resources
{
	public interface IToolbox
	{
		List<IItemDescriptor> Items { get; }
	}
}
