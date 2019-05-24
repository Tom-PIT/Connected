using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Reporting.UI;

namespace TomPIT.Reporting.Design.Items
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
