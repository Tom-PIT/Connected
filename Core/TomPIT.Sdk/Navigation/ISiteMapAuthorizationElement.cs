using System;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public interface ISiteMapAuthorizationElement
	{
		bool Authorize(IMiddlewareContext context, Guid user);
	}
}
