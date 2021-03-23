using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;

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