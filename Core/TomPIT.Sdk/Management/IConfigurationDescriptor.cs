namespace TomPIT.Management
{
	public interface IConfigurationDescriptor
	{
		string Text { get; }
		string Category { get; }
		IManagementSchemaProvider SchemaProvider { get; }
	}
}
