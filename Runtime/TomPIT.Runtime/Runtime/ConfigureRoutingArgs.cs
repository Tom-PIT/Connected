using System;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Runtime
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
