﻿using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Cdn;

namespace TomPIT.MicroServices.Design.Items
{
	internal class SubscriptionEventsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Event", "Event", typeof(SubscriptionEvent)));
		}
	}
}
