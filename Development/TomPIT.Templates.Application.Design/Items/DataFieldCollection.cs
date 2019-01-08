using System.Collections.Generic;
using TomPIT.Application.Data;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class DataFieldCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Bound field", "BoundField", typeof(BoundField)));
			items.Add(new ItemDescriptor("Unbound field", "UnboundField", typeof(UnboundField)));
		}
	}
}
