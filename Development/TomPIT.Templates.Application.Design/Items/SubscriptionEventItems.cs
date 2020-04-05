using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class SubscriptionEventItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			FillItems(element, items, element.Environment.Context.MicroService.Name);

			var references = element.Environment.Context.Tenant.GetService<IDiscoveryService>().References(element.Environment.Context.MicroService.Token);

			if (references == null)
				return;

			foreach (var reference in references.MicroServices)
				FillItems(element, items, reference.MicroService);
		}

		private void FillItems(IDomElement element, List<IItemDescriptor> items, string microService)
		{
			var ms = element.Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			var subscriptions = element.Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, ComponentCategories.Subscription);

			foreach (var subscription in subscriptions)
			{
				if (!(subscription is ISubscriptionConfiguration subConfig))
					continue;

				var componentName = subscription.ComponentName();

				foreach (var e in subConfig.Events)
				{
					var key = $"{ms.Name}/{componentName}/{e.Name}";

					items.Add(new ItemDescriptor($"{e.Name} ({componentName}/{ms.Name})", key));
				}
			}
		}
	}
}
