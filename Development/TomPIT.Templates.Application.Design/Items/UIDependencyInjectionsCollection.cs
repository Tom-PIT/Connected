using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.IoC;

namespace TomPIT.MicroServices.Design.Items
{
	internal class UIDependencyInjectionsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("View Dependency", "View Dependency", typeof(ViewDependency)));
			items.Add(new ItemDescriptor("Partial Dependency", "Partial Dependency", typeof(PartialDependency)));
			items.Add(new ItemDescriptor("Master Dependency", "Master Dependency", typeof(MasterDependency)));
		}
	}
}
