using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Resources;

namespace TomPIT.MicroServices.Design.Items
{
	internal class ScriptSourceCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Code", "Code", typeof(ScriptCodeSource)));
			items.Add(new ItemDescriptor("Upload", "Upload", typeof(ScriptUploadSource)));
			items.Add(new ItemDescriptor("File system", "File system", typeof(ScriptFileSystemSource)));
		}
	}
}
