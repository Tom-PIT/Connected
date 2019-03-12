using System.Collections.Generic;
using TomPIT.Application.Cdn;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class SubscriptionEventsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Event", "Event", typeof(SubscriptionEvent)));
		}
	}
}
