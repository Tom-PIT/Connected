namespace TomPIT.Management
{
	public interface IConfigurationDescriptor
	{
		string Partial { get; }
		string Name { get; }
		IManagementSchemaProvider SchemaProvider { get; }
	}
}
