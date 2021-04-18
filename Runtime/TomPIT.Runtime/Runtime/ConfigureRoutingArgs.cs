using System;
using Microsoft.AspNetCore.Routing;

namespace TomPIT.Runtime
{
	public class ConfigureRoutingArgs : EventArgs
	{
		public ConfigureRoutingArgs(IEndpointRouteBuilder builder)
		{
			Builder = builder;
		}

		public IEndpointRouteBuilder Builder { get; }
	}
}
