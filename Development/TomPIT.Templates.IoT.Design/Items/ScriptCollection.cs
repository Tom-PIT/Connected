using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.IoT.Items
{
	internal class ScriptCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Javascript", "Javascript", typeof(Javascript)));
		}
	}
}
