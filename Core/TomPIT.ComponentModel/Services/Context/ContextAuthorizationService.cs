using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Services.Context
{
	internal class ContextAuthorizationService : ContextClient, IContextAuthorizationService
	{
		public ContextAuthorizationService(IExecutionContext context) : base(context)
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
