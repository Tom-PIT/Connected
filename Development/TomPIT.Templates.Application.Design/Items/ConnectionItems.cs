using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Collections;

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
