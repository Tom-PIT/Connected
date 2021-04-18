using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Distributed;

namespace TomPIT.MicroServices.Design.Items
{
	internal class DistributedEventCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Distributed event", "Event", typeof(DistributedEvent)));
		}
	}
}
