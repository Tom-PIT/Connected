using System.Collections.Generic;
using TomPIT.Application.Data;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class DataSourceParameterCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Parameter", "Parameter", typeof(Parameter)));
		}
	}
}
