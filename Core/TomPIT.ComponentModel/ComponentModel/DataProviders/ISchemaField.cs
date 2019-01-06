namespace TomPIT.ComponentModel.DataProviders
{
	public interface ISchemaField
	{
		DataType DataType { get; }
		string Name { get; }
	}
}