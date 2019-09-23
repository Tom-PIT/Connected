using System;
using System.Security.Principal;
using TomPIT.Connectivity;
using TomPIT.Models;
using TomPIT.Security;

namespace TomPIT.Middleware
{
	public interface IMiddlewareDescriptor
	{
		IIdentity Identity { get; }
		Guid UserToken { get; }
		IUser User { get; }
		string JwToken { get; }
		ITenant Tenant { get; }

		string RouteUrl(IActionContextProvider provider, string routeName, object values);
		IMiddlewareContext CreateContext(Guid microService);
	}
}
