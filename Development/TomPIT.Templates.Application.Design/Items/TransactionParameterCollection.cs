using System.Collections.Generic;
using TomPIT.Application.Data;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	public class TransactionParameterCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Parameter", "Parameter", typeof(Parameter)));
			items.Add(new ItemDescriptor("Return Value", "ReturnValue", typeof(ReturnValueParameter)));
		}
	}
}
