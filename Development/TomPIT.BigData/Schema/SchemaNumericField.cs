using System.ComponentModel;
using TomPIT.Annotations;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Schema
{
	[Create("Field", nameof(Name))]
	public class SchemaNumericField : SchemaField, ISchemaNumericField
	{
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(NumericType.Int)]
		public NumericType Type { get; set; } = NumericType.Int;
		[PropertyCategory(PropertyCategoryAttribute.CategoryData)]
		[DefaultValue(NumericAggregateMode.None)]
		public NumericAggregateMode Aggregate { get; set; } = NumericAggregateMode.None;
	}
}
