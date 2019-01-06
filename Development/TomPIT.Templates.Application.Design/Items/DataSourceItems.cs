using System;
using System.Collections.Generic;
using TomPIT.Application.Data;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class DataSourceItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.Connection().GetService<IComponentService>().QueryComponents(element.Environment.Context.MicroService(), DataSource.ComponentCategory);

			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
