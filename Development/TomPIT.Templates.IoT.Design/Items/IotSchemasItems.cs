using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class IotSchemasItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var s = element.MicroService();
			var server = element.Environment.Context.Connection();

			var ds = server.GetService<IComponentService>().QueryComponents(s, "IoTSchema").OrderBy(f => f.Name);

			items.Add(Empty(SR.DevSelect, string.Empty));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
