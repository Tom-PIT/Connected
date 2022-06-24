using System;
using Microsoft.AspNetCore.Builder;

namespace TomPIT.Runtime
{
	public class ConfigureMiddlewareArgs : EventArgs
	{
		public ConfigureMiddlewareArgs(IApplicationBuilder builder)
		{
			Builder = builder;
		}

		public IApplicationBuilder Builder { get; }
	}
}
