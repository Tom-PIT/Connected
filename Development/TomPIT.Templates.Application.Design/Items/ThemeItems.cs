using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ThemeItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var s = element.MicroService();
			var tenant = element.Environment.Context.Tenant;
			var microService = tenant.GetService<IMicroServiceService>().Select(s);

			var ds = tenant.GetService<IComponentService>().QueryComponents(s, ComponentCategories.Theme).OrderBy(f => f.Name);

			items.Add(Empty(SR.ItemNone, string.Empty));

			foreach (var i in ds)
			{
				if (i.Token == Guid.Parse(element.Id))
					continue;

				items.Add(new ItemDescriptor($"{i.Name} ({microService.Name})", $"{microService.Name}/{i.Name}"));
			}

			var refs = tenant.GetService<IDiscoveryService>().MicroServices.References.Select(element.MicroService());

			var external = new List<ItemDescriptor>();

			foreach (var i in refs.MicroServices)
			{
				var ms = tenant.GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				ds = tenant.GetService<IComponentService>().QueryComponents(ms.Token, ComponentCategories.MasterView).OrderBy(f => f.Name);

				foreach (var j in ds)
				{
					var key = string.Format("{0}/{1}", ms.Name, j.Name);

					external.Add(new ItemDescriptor($"{j.Name} ({ms.Name})", key));
				}
			}

			if (external.Count > 0)
				items.AddRange(external.OrderBy(f => f.Text));
		}
	}
}
