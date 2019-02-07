using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public interface IDesignerToolbox : IEnvironmentClient
	{
		List<IItemDescriptor> Items { get; }
	}
}
