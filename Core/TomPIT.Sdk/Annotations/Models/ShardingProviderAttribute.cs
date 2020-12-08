using System;
using System.Collections.Generic;
using TomPIT.Data;
using TomPIT.Middleware;

namespace TomPIT.Annotations.Models
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class ShardingProviderAttribute : Attribute
	{

		public List<IConnectionString> QueryConnections(IMiddlewareContext context)
		{
			Context = context;

			return OnQueryConnections();
		}

		protected IMiddlewareContext Context { get; private set; }

		protected virtual List<IConnectionString> OnQueryConnections()
		{
			return null;
		}
	}
}
