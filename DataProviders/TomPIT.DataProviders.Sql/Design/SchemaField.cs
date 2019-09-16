using TomPIT.Data.DataProviders.Design;

namespace TomPIT.DataProviders.Sql.Design
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
