using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace TomPIT
{
	public delegate void ConfigureMvcHandler(MvcOptions e);
	public delegate void ApplicationPartsHandler(ApplicationPartsArgs e);

	public class ServicesConfigurationArgs : EventArgs
	{
		private List<string> _parts = null;

		public AuthenticationType Authentication { get; set; } = AuthenticationType.MultiTenant;
		public ConfigureMvcHandler ConfigureMvc { get; set; }
		public ApplicationPartsHandler ProvideApplicationParts { get; set; }
	}
}
