using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public interface ICollectionDesigner : IDomDesigner, IEnvironmentClient, IDomClient
	{
		List<IItemDescriptor> Items { get; }
		List<IItemDescriptor> Descriptors { get; }
		bool SupportsReorder { get; }
	}
}