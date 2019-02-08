using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class IoTSchemaFieldsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Field", "Field", typeof(IoTSchemaField)));
		}
	}
}
