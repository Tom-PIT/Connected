using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.IoT.UI;

namespace TomPIT.MicroServices.IoT.Design.Items
{
	internal class ViewHelpersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Helper", "helper", typeof(ViewHelper)));
		}
	}
}
