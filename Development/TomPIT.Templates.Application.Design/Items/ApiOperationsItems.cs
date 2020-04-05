using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ApiOperationsItems : ItemsBase
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

			var apis = element.Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, ComponentCategories.Api);

			foreach (var api in apis)
			{
				if (!(api is IApiConfiguration apiConfig))
					continue;

				var componentName = api.ComponentName();

				foreach (var op in apiConfig.Operations)
				{
					var key = $"{ms.Name}/{componentName}/{op.Name}";

					items.Add(new ItemDescriptor($"{op.Name} ({componentName}/{ms.Name})", key));
				}
			}
		}
	}
}
