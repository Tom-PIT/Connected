using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Apis;

namespace TomPIT.MicroServices.Design.Items
{
	internal class OperationCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Operation", "Operation", typeof(Operation)));
		}
	}
}
