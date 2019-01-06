using System.Collections.Generic;
using TomPIT.Application.Resources;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class ScriptSourceCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Upload", "Upload", typeof(ScriptUploadSource)));
			items.Add(new ItemDescriptor("Code", "Code", typeof(ScriptCodeSource)));
			items.Add(new ItemDescriptor("File system", "File system", typeof(ScriptFileSystemSource)));
		}
	}
}
