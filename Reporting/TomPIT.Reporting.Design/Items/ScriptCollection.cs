using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Reporting.Design.Items
{
	internal class ScriptCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Javascript", "Javascript", typeof(Javascript)));
		}
	}
}
