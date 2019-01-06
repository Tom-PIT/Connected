using TomPIT.Data.DataProviders;

namespace TomPIT.DataProviders.Sql
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
