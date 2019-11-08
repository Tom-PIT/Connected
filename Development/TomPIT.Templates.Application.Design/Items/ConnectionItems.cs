using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ConnectionItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.Tenant.GetService<IComponentService>().QueryComponents(element.MicroService(), ComponentCategories.Connection);

			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token));
		}
	}
}
