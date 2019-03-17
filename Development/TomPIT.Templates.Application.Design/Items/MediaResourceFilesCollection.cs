using System.Collections.Generic;
using TomPIT.Application.Resources;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class MediaResourceFilesCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("File", "File", typeof(MediaResourceFile)));
		}
	}
}
