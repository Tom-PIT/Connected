using System.Collections.Generic;
using TomPIT.Ide.Collections;

namespace TomPIT.Ide.Environment.Providers
{
	public interface IToolboxProvider : IEnvironmentObject
	{
		List<IItemDescriptor> Items { get; }
	}
}
