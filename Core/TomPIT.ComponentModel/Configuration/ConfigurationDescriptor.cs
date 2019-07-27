using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Configuration
{
	public class ConfigurationDescriptor
	{
		public IMicroService MicroService { get; private set; }
		public IComponent Component { get; private set; }
		public IConfiguration Configuration { get; private set; }

		public static ConfigurationDescriptor Parse(ISysConnection connection, string qualifier, string componentCategory)
		{
			var result = new ConfigurationDescriptor();
			var tokens = qualifier.Split("/".ToCharArray(), 2);

			result.MicroService = connection.GetService<IMicroServiceService>().Select(tokens[0]);

			if (result.MicroService == null)
				return result;

			result.Component = connection.GetService<IComponentService>().SelectComponent(result.MicroService.Token, componentCategory, tokens[1]);

			if (result.Component == null)
				return result;

			result.Configuration = connection.GetService<IComponentService>().SelectConfiguration(result.Component.Token);

			return result;
		}
	}
}
