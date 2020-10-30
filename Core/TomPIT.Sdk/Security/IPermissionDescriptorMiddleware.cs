using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public interface IPermissionDescriptorMiddleware : IMiddlewareObject
	{
		AuthorizationProviderResult Authorize(IPermission permission, AuthorizationArgs e, Dictionary<string, object> state);
	}
}
