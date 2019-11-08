using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Runtime;

namespace TomPIT.MicroServices.Apis
{
	internal class ApiProtocolOptions : ConfigurationElement, IApiProtocolOptions
	{
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public bool Rest { get; set; }
	}
}
