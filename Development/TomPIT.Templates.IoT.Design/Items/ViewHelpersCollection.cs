using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.IoT.UI;

namespace TomPIT.IoT.Design.Items
{
	internal class ViewHelpersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Helper", "helper", typeof(ViewHelper)));
		}
	}
}
