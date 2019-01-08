using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TomPIT
{
	public class ServicesConfigurationArgs:EventArgs
	{
		private List<Assembly> _assemblies = null;

		public AuthenticationType Authentication { get; set; } = AuthenticationType.Jwt;
		
		public List<Assembly> ApplicationParts
		{
			get
			{
				if (_assemblies == null)
					_assemblies = new List<Assembly>();

				return _assemblies;
			}
		}
	}
}
