using System.Collections.Generic;
using TomPIT.Application.UI;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Design.Items
{
	internal class StylesheetCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Less", "Less", typeof(LessFile)));
			items.Add(new ItemDescriptor("Css", "Css", typeof(CssFile)));
			items.Add(new ItemDescriptor("File system Css", "FileCss", typeof(FileSystemCssFile)));
			items.Add(new ItemDescriptor("Include", "Include", typeof(LessIncludeFile)));
		}
	}
}
