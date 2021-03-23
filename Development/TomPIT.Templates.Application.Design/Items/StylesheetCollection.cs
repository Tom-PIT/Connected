using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;
using TomPIT.MicroServices.UI.Theming;

namespace TomPIT.MicroServices.Design.Items
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
