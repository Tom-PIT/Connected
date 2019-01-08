using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Environment;

namespace TomPIT.Items
{
	internal class EndpointsCollection : ItemsBase
	{
		public const string Endpoints = "{E6DB602E-2C5E-4D22-8CFF-66B300211CAF}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Endpoint", Endpoints, typeof(IInstanceEndpoint)));
		}
	}
}
