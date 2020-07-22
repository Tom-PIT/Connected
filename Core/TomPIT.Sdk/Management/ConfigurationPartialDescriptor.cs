using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Management
{
	public class ConfigurationPartialDescriptor : ConfigurationDescriptor, IConfigurationPartialDescriptor
	{
		[CIP(CIP.PartialProvider)]
		public string Partial { get; set; }
	}
}
