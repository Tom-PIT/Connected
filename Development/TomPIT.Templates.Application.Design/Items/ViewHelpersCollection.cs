using System.Collections.Generic;
using TomPIT.Application.UI;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class ViewHelpersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Helper", "helper", typeof(ViewHelper)));
		}
	}
}
