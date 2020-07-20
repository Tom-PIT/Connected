using TomPIT.Middleware;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Management
{
	public class ConfigurationDescriptor : MiddlewareObject, IConfigurationDescriptor
	{
		[CIP(CIP.PartialProvider)]
		public string Partial { get; set; }
		public string Name { get; set; }
		public IManagementSchemaProvider SchemaProvider { get; set; }
	}
}
