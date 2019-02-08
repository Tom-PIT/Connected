using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class IoTDeviceTransactionsItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var e = element.Component as IElement;
			var hub = e.Closest<IIoTHub>();

			if (string.IsNullOrWhiteSpace(hub.Schema))
				return;

			var schema = hub.ResolveSchema(element.Environment.Context);

			if (schema == null)
				return;

			foreach (var i in schema.Transactions.Where(f => f.Scope == IoTTransactonScope.Scoped && f.Validation.Validate(element.Environment.Context)))
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
