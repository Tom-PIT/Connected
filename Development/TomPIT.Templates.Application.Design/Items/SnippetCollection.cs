using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.UI;

namespace TomPIT.MicroServices.Design.Items
{
	internal class SnippetCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Snippet", "Snippet", typeof(Snippet)));
		}
	}
}
