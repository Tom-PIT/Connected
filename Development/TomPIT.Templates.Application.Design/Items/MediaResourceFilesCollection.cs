using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
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
