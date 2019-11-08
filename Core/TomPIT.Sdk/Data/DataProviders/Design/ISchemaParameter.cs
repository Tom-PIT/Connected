namespace TomPIT.Data.DataProviders.Design
{
	public interface ISchemaParameter
	{
		DataType DataType { get; }
		string Name { get; }
		bool IsNullable { get; }
	}
}