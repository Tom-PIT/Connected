using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.IoT.Design.Items
{
	internal class DataMemberItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			if (!(element.Component is IElement e))
				return;

			var view = e.Closest<IIoTView>();
			var hub = view.ResolveHub(element.Environment.Context);

			if (string.IsNullOrWhiteSpace(hub.Schema))
				return;

			var schema = hub.ResolveSchema(element.Environment.Context);

			if (schema == null)
				return;

			items.Add(new ItemDescriptor(SR.DevSelect, string.Empty));

			foreach (var i in schema.Fields)
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
