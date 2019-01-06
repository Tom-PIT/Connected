using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT
{
	public class ConfigureRoutingArgs : EventArgs
	{
		public ConfigureRoutingArgs(IRouteBuilder builder)
		{
			Builder = builder;
		}

		public IRouteBuilder Builder { get; }
	}
}
