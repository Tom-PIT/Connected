using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.Design.Items
{
	internal class ReferencesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Reference", "reference", typeof(ServiceReference)));
		}
	}
}
