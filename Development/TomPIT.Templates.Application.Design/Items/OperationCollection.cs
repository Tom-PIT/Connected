using System.Collections.Generic;
using TomPIT.Application.Apis;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class OperationCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Operation", "Operation", typeof(Operation)));
		}
	}
}
