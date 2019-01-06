namespace TomPIT.Data.DataProviders
{
	public interface ISchemaParameter
	{
		DataType DataType { get; }
		string Name { get; }
		bool IsNullable { get; }
	}
}