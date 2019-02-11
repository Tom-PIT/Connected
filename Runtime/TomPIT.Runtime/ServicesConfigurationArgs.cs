using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace TomPIT
{
	public delegate void ConfigureMvcHandler(MvcOptions e);

	public class ServicesConfigurationArgs : EventArgs
	{
		private List<string> _parts = null;

		public AuthenticationType Authentication { get; set; } = AuthenticationType.MultiTenant;
		public ConfigureMvcHandler ConfigureMvc { get; set; }

		public List<string> ApplicationParts
		{
			get
			{
				if (_parts == null)
					_parts = new List<string>();

				return _parts;
			}
		}
	}
}
