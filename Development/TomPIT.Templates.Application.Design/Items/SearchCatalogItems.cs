using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Search;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class SearchCatalogItems : ItemsBase
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

			var catalogs = element.Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, ComponentCategories.SearchCatalog);

			foreach (var catalog in catalogs)
			{
				if (!(catalog is ISearchCatalogConfiguration apiConfig))
					continue;

				var componentName = catalog.ComponentName();

				var key = $"{ms.Name}/{componentName}";

				items.Add(new ItemDescriptor($"{componentName} ({ms.Name})", key));
			}
		}
	}
}
