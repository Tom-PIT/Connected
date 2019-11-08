using System;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareAuthorizationService : MiddlewareComponent, IMiddlewareAuthorizationService
	{
		public MiddlewareAuthorizationService(IMiddlewareContext context) : base(context)
		{
		}

		public bool Authorize(string claim, string primaryKey)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Authorize(claim, primaryKey, user);
		}

		public bool Authorize(string claim, string primaryKey, Guid user)
		{
			return Context.Tenant.GetService<IAuthorizationService>().Authorize(Context, new AuthorizationArgs(user, claim, primaryKey)).Success;
		}
	}
}
