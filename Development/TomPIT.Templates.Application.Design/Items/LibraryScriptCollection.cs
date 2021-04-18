using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Scripting;

namespace TomPIT.MicroServices.Design.Items
{
	internal class LibraryScriptCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("C#", "csharp", typeof(CSharpScript)));
		}
	}
}
