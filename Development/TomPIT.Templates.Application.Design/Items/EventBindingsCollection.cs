using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Messaging;

namespace TomPIT.MicroServices.Design.Items
{
	internal class EventBindingsCollection : ItemsBase
	{
		public const string Binding = "{0624A056-47D4-4EAB-89D2-4160A0BE5246}";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Event binding", Binding, typeof(EventBinding)));
		}
	}
}
