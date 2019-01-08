using System;
using System.Collections.Generic;
using TomPIT.Data.DataProviders;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class DataProviderItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.Connection().GetService<IDataProviderService>().Query();

			items.Add(new ItemDescriptor(SR.DevSelect, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Id.ToString()));
		}
	}
}
