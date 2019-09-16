using System;
using TomPIT.Development;
using TomPIT.Middleware;

namespace TomPIT.Design.Tools
{
	public class AutoFixArgs : EventArgs
	{
		public AutoFixArgs(IMiddlewareContext context, IDevelopmentError error)
		{
			Context = context;
			Error = error;
		}

		public IMiddlewareContext Context { get; }
		public IDevelopmentError Error { get; }
	}
}
