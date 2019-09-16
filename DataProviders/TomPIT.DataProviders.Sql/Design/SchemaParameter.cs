using TomPIT.Data.DataProviders.Design;

namespace TomPIT.DataProviders.Sql.Design
{
	internal class SchemaParameter : ISchemaParameter
	{
		public DataType DataType
		{
			get; set;
		}

		public bool IsNullable
		{
			get; set;
		}

		public string Name
		{
			get; set;
		}
	}
}
