using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Reporting.UI;

namespace TomPIT.Reporting.Design.Items
{
	internal class ViewHelpersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Helper", "helper", typeof(ViewHelper)));
		}
	}
}
