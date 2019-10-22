using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class IoCContainerItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor(SR.DevSelect, string.Empty));

			FillContainers(element, items, element.Environment.Context.MicroService.Name);

			var references = element.Environment.Context.Tenant.GetService<IDiscoveryService>().References(element.Environment.Context.MicroService.Name);

			if (references != null)
			{
				foreach (var reference in references.MicroServices)
					FillContainers(element, items, reference.MicroService);
			}
		}

		private void FillContainers(IDomElement element, List<IItemDescriptor> items, string microService)
		{
			var ms = element.Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				return;

			var ds = element.Environment.Context.Tenant.GetService<IComponentService>().QueryConfigurations(ms.Token, ComponentCategories.IoCContainer);

			foreach (var i in ds)
			{
				var name = i.ComponentName();

				if (i is IIoCContainerConfiguration container)
				{
					foreach (var operation in container.Operations)
					{
						if (string.IsNullOrWhiteSpace(operation.Name))
							continue;

						var key = $"{ms.Name}/{name}/{operation.Name}";

						items.Add(new ItemDescriptor($"{operation.Name} ({name}, {ms.Name})", key));
					}
				}
			}
		}
	}
}
