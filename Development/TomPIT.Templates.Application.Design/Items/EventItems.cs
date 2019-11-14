using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class EventItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(Empty(SR.DevSelect, string.Empty));
			var ms = element.Environment.Context.Tenant.GetService<IMicroServiceService>().Select(element.MicroService());

			if (ms == null)
				return;

			BindMicroService(element, items, ms.Name);

			var refs = element.Environment.Context.Tenant.GetService<IDiscoveryService>().References(ms.Token);

			if (refs == null || refs.MicroServices.Count == 0)
				return;

			foreach (var reference in refs.MicroServices)
				BindMicroService(element, items, reference.MicroService);
		}

		private void BindMicroService(IDomElement element, List<IItemDescriptor> items, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return;

			var ms = element.Environment.Context.Tenant.GetService<IMicroServiceService>().Select(name);

			if (ms == null)
				return;

			var components = element.Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.DistributedEvent).OrderBy(f => f.Name);

			foreach (var component in components)
				BindComponent(element, items, ms, component);
		}

		private void BindComponent(IDomElement element, List<IItemDescriptor> items, IMicroService microService, IComponent component)
		{
			var configuration = element.Environment.Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) as IDistributedEventsConfiguration;

			if (configuration == null)
				return;

			foreach (var e in configuration.Events)
			{
				var value = $"{microService.Name}/{component.Name}/{e.Name}";

				items.Add(new ItemDescriptor(value, value));
			}
		}
	}
}
