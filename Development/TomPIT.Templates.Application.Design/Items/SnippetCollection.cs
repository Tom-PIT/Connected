using System.Collections.Generic;
using TomPIT.Application.UI;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class SnippetCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Snippet", "Snippet", typeof(Snippet)));
		}
	}
}
