using TomPIT.Annotations;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData.Schema
{
	[Create("Field", nameof(Name))]
	public class SchemaBoolField : SchemaField, ISchemaBoolField
	{
	}
}
