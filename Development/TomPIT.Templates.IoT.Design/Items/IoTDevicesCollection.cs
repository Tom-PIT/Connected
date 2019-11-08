using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.IoT.Design.Items
{
	internal class IoTDevicesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("IoT Device", "IoTDevice", typeof(IoTDevice)));
		}
	}
}
