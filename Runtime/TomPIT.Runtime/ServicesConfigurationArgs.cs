using Microsoft.AspNetCore.Mvc;
using System;

namespace TomPIT
{
	public delegate void ConfigureMvcHandler(MvcOptions e);

	public class ServicesConfigurationArgs : EventArgs
	{
		public AuthenticationType Authentication { get; set; } = AuthenticationType.MultiTenant;
		public ConfigureMvcHandler ConfigureMvc { get; set; }
	}
}
