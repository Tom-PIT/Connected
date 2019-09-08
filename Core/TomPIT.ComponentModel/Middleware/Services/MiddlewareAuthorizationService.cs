using System;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareAuthorizationService : MiddlewareComponent, IMiddlewareAuthorizationService
	{
		public MiddlewareAuthorizationService(IDataModelContext context) : base(context)
		{
		}

		public bool Authorize(string claim, string primaryKey)
		{
			return Authorize(claim, primaryKey, Context.GetAuthenticatedUserToken());
		}

		public bool Authorize(string claim, string primaryKey, Guid user)
		{
			return Context.Connection().GetService<IAuthorizationService>().Authorize(Context, new AuthorizationArgs(user, claim, primaryKey)).Success;
		}
	}
}
