using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Items
{
	internal class MicroServicesCollection : ItemsBase
	{
		public const string MicroService = "{FE260FFE-5881-4985-8EC8-D0B662C834B4}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Microservice", MicroService, typeof(IMicroService)));
		}
	}
}
