using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.IoT.Design.Items
{
	internal class DataMemberItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			if (!(element.Component is IElement e))
				return;

			var view = e.Closest<IIoTViewConfiguration>();
			var hub = view.ResolveHub(element.Environment.Context);
			var schemaType = hub.IoTHubSchemaType(element.Environment.Context);

			items.Add(new ItemDescriptor(SR.DevSelect, string.Empty));

			if (schemaType == null)
				return;

			var properties = ConfigurationExtensions.GetMiddlewareProperties(schemaType, false);

			foreach (var i in properties)
				items.Add(new ItemDescriptor(i.Name, i.Name));
		}
	}
}
