using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class LibraryScriptCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("C#", "csharp", typeof(CSharpScript)));
		}
	}
}
