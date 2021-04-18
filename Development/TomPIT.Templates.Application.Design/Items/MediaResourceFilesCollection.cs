using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.Resources;

namespace TomPIT.MicroServices.Design.Items
{
	internal class MediaResourceFilesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("File", "File", typeof(MediaResourceFile)));
		}
	}
}
