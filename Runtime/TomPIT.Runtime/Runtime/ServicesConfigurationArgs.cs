using System;
using Microsoft.AspNetCore.Mvc;

namespace TomPIT.Runtime
{
	public delegate void ConfigureMvcHandler(MvcOptions e);
	public delegate void ApplicationPartsHandler(ApplicationPartsArgs e);

	public class ServicesConfigurationArgs : EventArgs
	{
		public AuthenticationType Authentication { get; set; } = AuthenticationType.MultiTenant;
		public ConfigureMvcHandler ConfigureMvc { get; set; }
		public ApplicationPartsHandler ProvideApplicationParts { get; set; }
	}
}
