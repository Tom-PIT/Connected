using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.IoC;

namespace TomPIT.MicroServices.Design.Items
{
	internal class DependencyInjectionsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("API Operation", "API Operation", typeof(ApiDependency)));
			items.Add(new ItemDescriptor("Search Catalog", "Search Catalog", typeof(SearchDependency)));
			items.Add(new ItemDescriptor("Subscription", "Subscription", typeof(SubscriptionDependency)));
			items.Add(new ItemDescriptor("Subscription Event", "Subscription Event", typeof(SubscriptionEventDependency)));
		}
	}
}
