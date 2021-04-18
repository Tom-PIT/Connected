using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;

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
