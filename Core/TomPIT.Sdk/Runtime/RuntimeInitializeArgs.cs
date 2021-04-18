using System;
using Microsoft.AspNetCore.Builder;

namespace TomPIT.Runtime
{
	public class RuntimeInitializeArgs : EventArgs
	{
		public RuntimeInitializeArgs(IApplicationBuilder builder)
		{
			Builder = builder;
		}

		public IApplicationBuilder Builder { get; }
	}
}
