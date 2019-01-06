namespace TomPIT.Data.DataProviders
{
	public interface ISchemaField
	{
		DataType DataType { get; }
		string Name { get; }
	}
}