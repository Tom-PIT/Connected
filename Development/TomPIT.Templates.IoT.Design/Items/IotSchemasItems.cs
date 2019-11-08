using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.IoT.Design.Items
{
	internal class IoTSchemasItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var s = element.MicroService();
			var tenant = element.Environment.Context.Tenant;

			var ds = tenant.GetService<IComponentService>().QueryComponents(s, "IoTSchema").OrderBy(f => f.Name);

			items.Add(Empty(SR.DevSelect, string.Empty));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
