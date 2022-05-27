using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ConnectionItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			FillItems(element, items, element.Environment.Context.MicroService.Name);

			if( element.Environment.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(element.Environment.Context.MicroService.Token) is not IServiceReferencesConfiguration references)
				return;

			foreach (var reference in references.MicroServices)
				FillItems(element, items, reference.MicroService);
		}

		private void FillItems(IDomElement element, List<IItemDescriptor> items, string microService)
		{
			if( element.Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService) is not IMicroService ms)
				return;

			var connections = element.Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.Connection);

			foreach (var connection in connections)
			{
				var key = $"{ms.Name}/{connection.Name}";

				items.Add(new ItemDescriptor($"{connection.Name} ({ms.Name})", key));
			}
		}
	}
}
