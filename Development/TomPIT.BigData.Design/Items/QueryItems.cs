using System.Collections.Generic;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Ide.Collections;

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
