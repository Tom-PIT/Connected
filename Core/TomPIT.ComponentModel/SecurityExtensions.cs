using System;
using System.Net;
using TomPIT.ComponentModel;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT
{
	public static class SecurityExtensions
	{
		public static bool Authorize(this IExecutionContext context, IComponent component)
		{
			if (component == null)
				return false;

			var e = new AuthorizationArgs(context.GetAuthenticatedUserToken(), Claims.AccessUserInterface, component.Token.ToString());

			e.Schema.Empty = EmptyBehavior.Deny;
			e.Schema.Level = AuthorizationLevel.Pessimistic;

			var r = context.Connection().GetService<IAuthorizationService>().Authorize(context, e);

			if (r.Success)
				return true;

			if (r.Reason == AuthorizationResultReason.Empty)
			{
				if (context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IAuthorizationChain v
					&& v.AuthorizationParent != Guid.Empty)
				{
					var c = context.Connection().GetService<IComponentService>().SelectComponent(v.AuthorizationParent);

					if (c != null)
						return Authorize(context, c);
				}
				else
					Reject(context);
			}
			else
				Reject(context);

			return false;
		}

		private static void Reject(IExecutionContext context)
		{
			if (context.GetAuthenticatedUserToken() == Guid.Empty)
				Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else
				Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}
	}
}
