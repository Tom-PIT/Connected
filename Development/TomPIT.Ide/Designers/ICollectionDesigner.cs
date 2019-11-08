using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment;

namespace TomPIT.Ide.Designers
{
	public interface ICollectionDesigner : IDomDesigner, IEnvironmentObject, IDomObject
	{
		List<IItemDescriptor> Items { get; }
		List<IItemDescriptor> Descriptors { get; }
		bool SupportsReorder { get; }
		string ItemTemplateView { get; }
	}
}