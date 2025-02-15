﻿using System.Collections.Generic;
using System.Linq;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Reflection;

namespace TomPIT.Design.Items
{
	internal class MicroServicesItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ms = element.MicroService();
			var s = element.Environment.Context.Tenant.GetService<IMicroServiceService>().Query().OrderBy(f => f.Name);
			var existing = ((IElement)element.Component).Closest<ListItems<IServiceReference>>();
			var value = element.Component as IServiceReference;

			items.Add(Empty(SR.DevSelect, string.Empty.ToString()));

			foreach (var i in s)
			{
				if (i.Token == ms)
					continue;

				if (string.Compare(value.MicroService, i.Name, true) == 0)
					items.Add(new ItemDescriptor(i.Name, i.Name.ToString()));

				if (existing.FirstOrDefault(f => string.Compare(f.MicroService, i.Name, true) == 0) != null)
					continue;

				items.Add(new ItemDescriptor(i.Name, i.Name.ToString()));
			}
		}
	}
}
