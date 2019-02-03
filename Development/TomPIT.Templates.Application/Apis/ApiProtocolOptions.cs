using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;

namespace TomPIT.Application.Apis
{
	internal class ApiProtocolOptions : ConfigurationElement, IApiProtocolOptions
	{
		[EnvironmentVisibility(EnvironmentMode.Runtime)]
		public bool Rest { get; set; }
	}
}
