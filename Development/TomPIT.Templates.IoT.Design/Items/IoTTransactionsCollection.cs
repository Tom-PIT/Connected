using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.IoT.Design.Items
{
	internal class IoTTransactionsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("IoT Transaction", "IoTTransaction", typeof(IoTTransaction)));
		}
	}
}
