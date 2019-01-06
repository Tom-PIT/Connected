using System;
using System.Security.Claims;
using System.Security.Principal;
using TomPIT.Net;
using TomPIT.Runtime;
using TomPIT.Security;

namespace TomPIT.Server.Security
{
	public class Principal : ClaimsPrincipal
	{
		private ClaimsIdentity _identity = null;

		public Principal(ClaimsIdentity identity)
		{
			_identity = identity;

			AddIdentity(_identity);
		}

		public override IIdentity Identity { get { return _identity; } }

		public override bool IsInRole(string role)
		{
			if (!(Identity is Identity identity))
				return false;

			if (string.IsNullOrWhiteSpace(identity.Endpoint) && Shell.GetService<IRuntimeService>().Environment == RuntimeEnvironment.MultiTenant)
				return false;

			var ctx = Shell.GetService<IConnectivityService>().Select(identity.Endpoint);

			if (ctx == null)
				return false;

			var u = identity == null || identity.User == null
				? Guid.Empty
				: identity.User.Token;

			if (ctx.GetService<IAuthorizationService>().IsInRole(u, "Full Control"))
				return true;

			return ctx.GetService<IAuthorizationService>().IsInRole(u, role);
		}
	}
}
