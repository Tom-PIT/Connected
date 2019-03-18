using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Schema
{
	[Create("Field", nameof(Name))]
	public class SchemaDateField : SchemaField, ISchemaDateField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(DateType.DateTime)]
		public DateType Type { get; set; } = DateType.DateTime;
	}
}
