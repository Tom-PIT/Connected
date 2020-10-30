using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.MicroServices.BigData.Design.Items
{
	internal class QueryItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Sql Query", "SqlQuery", typeof(BigDataSqlQuery)));
		}
	}
}
