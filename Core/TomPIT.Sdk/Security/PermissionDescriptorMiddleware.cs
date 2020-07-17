using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public abstract class PermissionDescriptorMiddleware : MiddlewareObject, IPermissionDescriptorMiddleware
	{
		public AuthorizationProviderResult Authorize(IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			Arguments = e;
			State = state;

			return OnAuthorize(permission);
		}

		protected AuthorizationArgs Arguments { get; private set; }
		protected Dictionary<string, object> State { get; private set; }

		protected virtual AuthorizationProviderResult OnAuthorize(IPermission permission)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(AuthorizationArgs e, Dictionary<string, object> state)
		{
			Arguments = e;
			State = state;

			return OnPreAuthorize();
		}

		protected virtual AuthorizationProviderResult OnPreAuthorize()
		{
			return AuthorizationProviderResult.NotHandled;
		}
	}
}
