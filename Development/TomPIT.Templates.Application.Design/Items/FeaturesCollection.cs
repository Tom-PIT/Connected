using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Features;

namespace TomPIT.MicroServices.Design.Items
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
