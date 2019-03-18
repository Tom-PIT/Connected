using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Schema
{
	[Create("Field", nameof(Name))]
	public class SchemaStringField : SchemaField, ISchemaStringField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(128)]
		public int Length { get; set; } = 128;
	}
}
