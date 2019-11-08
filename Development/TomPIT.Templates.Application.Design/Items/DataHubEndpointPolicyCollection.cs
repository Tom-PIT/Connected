using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Cdn;

namespace TomPIT.MicroServices.Design.Items
{
	internal class DataHubEndpointPolicyCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Policy", "Policy", typeof(DataHubEndpointPolicy)));
		}
	}
}
