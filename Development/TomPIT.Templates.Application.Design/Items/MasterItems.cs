using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class MasterItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			FillItems(element, items, element.Environment.Context.MicroService.Name);

			var references = element.Environment.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(element.Environment.Context.MicroService.Token);

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

			var views = element.Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.MasterView);

			foreach (var view in views)
			{
				var key = $"{ms.Name}/{view.Name}";

				items.Add(new ItemDescriptor($"{view.Name} ({ms.Name})", key));
			}
		}
	}
}
