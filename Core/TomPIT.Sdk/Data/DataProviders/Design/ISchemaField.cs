namespace TomPIT.Data.DataProviders.Design
{
	public interface ISchemaField
	{
		DataType DataType { get; }
		string Name { get; }
	}
}