namespace TomPIT.Data.DataProviders.Design
{
	public class SchemaParameter : ISchemaParameter
	{
		public DataType DataType { get; set; }

		public string Name { get; set; }

		public bool IsNullable { get; set; }
	}
}
