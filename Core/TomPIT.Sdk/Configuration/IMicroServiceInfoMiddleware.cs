using System;
using TomPIT.Middleware;

namespace TomPIT.Configuration
{
	public interface IMicroServiceInfoMiddleware : IMiddlewareComponent
	{
		Version Version { get; }
		string Author { get; }
	}
}
