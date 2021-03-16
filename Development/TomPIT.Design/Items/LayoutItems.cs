using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.Design.Items
{
	internal class LayoutItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var s = element.MicroService();
			var tenant = element.Environment.Context.Tenant;

			var ds = tenant.GetService<IComponentService>().QueryComponents(s, "MasterView").OrderBy(f => f.Name);

			items.Add(Empty(SR.MasterDefault, string.Empty));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Name));

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

					external.Add(new ItemDescriptor(key, key));
				}
			}

			if (external.Count > 0)
				items.AddRange(external.OrderBy(f => f.Text));
		}
	}
}
