using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class IoTTransactionsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("IoT Transaction", "IoTTransaction", typeof(IoTTransaction)));
		}
	}
}
