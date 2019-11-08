using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.IoC;

namespace TomPIT.MicroServices.Design.Items
{
	internal class IoCEndpointCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Endpoint", "Endpoint", typeof(IoCEndpoint)));
		}
	}
}
