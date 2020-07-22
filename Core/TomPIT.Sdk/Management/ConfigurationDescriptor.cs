using TomPIT.Middleware;

namespace TomPIT.Management
{
	public abstract class ConfigurationDescriptor : MiddlewareObject, IConfigurationDescriptor
	{
		public string Text { get; set; }
		public string Category { get; set; }
		public IManagementSchemaProvider SchemaProvider { get; set; }
	}
}
