using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class IoTTransactionParametersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Parameter", "Parameter", typeof(IoTTransactionParameter)));
		}
	}
}
