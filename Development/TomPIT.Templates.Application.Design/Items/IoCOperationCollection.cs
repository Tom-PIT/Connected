using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.IoC;

namespace TomPIT.MicroServices.Design.Items
{
	internal class IoCOperationCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Operation", "Operation", typeof(IoCOperation)));
		}
	}
}
