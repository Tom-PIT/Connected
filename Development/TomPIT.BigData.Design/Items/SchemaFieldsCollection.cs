using System.Collections.Generic;
using TomPIT.BigData.Schema;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.BigData.Design.Items
{
	internal class SchemaFieldsCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("String", "String", typeof(SchemaStringField)));
			items.Add(new ItemDescriptor("Date", "Date", typeof(SchemaDateField)));
			items.Add(new ItemDescriptor("Numeric", "Numeric", typeof(SchemaNumericField)));
			items.Add(new ItemDescriptor("Bool", "Bool", typeof(SchemaBoolField)));
		}
	}
}
