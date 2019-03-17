namespace TomPIT.ComponentModel.BigData
{
	public interface ISchemaField : IConfigurationElement
	{
		string Name { get; }
		bool IsKey { get; }
		bool Index { get; }
	}
}
