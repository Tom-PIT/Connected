using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Data;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ModelViewsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("View", "View", typeof(ViewOperation)));
		}
	}
}
