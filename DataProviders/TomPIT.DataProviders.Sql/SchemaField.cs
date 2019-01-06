using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
{
	internal class SchemaField : ISchemaField
	{
		public DataType DataType
		{
			get; set;
		}

		public string Name
		{
			get; set;
		}
	}
}
