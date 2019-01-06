using System.Collections.Generic;
using TomPIT.Application.Data;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class DataManagementCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Group", "Group", typeof(DataManagementGroup)));
			items.Add(new ItemDescriptor("Descriptor", "Descriptor", typeof(DataManagementDescriptor)));
		}
	}
}
