using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Application.Features;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class FeaturesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("View feature", "ViewFeature", typeof(ViewFeature)));
			items.Add(new ItemDescriptor("Setting feature", "SettingFeature", typeof(SettingFeature)));
		}
	}
}
