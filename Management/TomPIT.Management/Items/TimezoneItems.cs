using System;
using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Items
{
	internal class TimezoneItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = TimeZoneInfo.GetSystemTimeZones();

			items.Add(new ItemDescriptor(SR.DevLiDefault, string.Empty));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.DisplayName, i.DisplayName));
		}
	}
}
