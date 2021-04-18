using System;
using System.Net;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public static class SecurityExtensions
	{
		public static bool MustChangePassword(this IAuthenticationResult result)
		{
			return !result.Success
				&& (result.Reason == AuthenticationResultReason.PasswordExpired || result.Reason == AuthenticationResultReason.NoPassword);
		}

		public static bool AuthorizeUrl(IMiddlewareContext context, string url, Guid user, bool setResponse = true)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			var tokens = url.Split('/');
			var path = string.Empty;
			var permissionCounter = 0;

			var defaultAr = AuthorizeDefaultUrl(context, user);

			if (!defaultAr.Success)
			{
				if (setResponse)
					Reject(context);

				return false;
			}

			permissionCounter += defaultAr.PermissionCount;

			for (var i = 0; i < tokens.Length; i++)
			{
				if (i > 0)
					path += "/";

				path += tokens[i];

				var empty = i == tokens.Length - 1 ? EmptyBehavior.Deny : EmptyBehavior.Alow;
				var token = user;
				var ar = AuthorizeUrl(context, new AuthorizationArgs(token, Claims.AccessUrl, path, "Url"), empty);

				if (ar.Success)
					permissionCounter += ar.PermissionCount;
				else
				{
					if (empty == EmptyBehavior.Deny && ar.Reason == AuthorizationResultReason.Empty && permissionCounter > 0)
						return true;
					else
					{
						if (setResponse)
							Reject(context);

						return false;
					}
				}
			}

			return true;
		}

		private static IAuthorizationResult AuthorizeDefaultUrl(IMiddlewareContext context, Guid user)
		{
			var args = new AuthorizationArgs(user, Claims.DefaultAccessUrl, 0.ToString(), "Default Url");

			args.Schema.Empty = EmptyBehavior.Alow;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			return context.Tenant.GetService<IAuthorizationService>().Authorize(context, args);
		}

		private static IAuthorizationResult AuthorizeUrl(IMiddlewareContext context, AuthorizationArgs e, EmptyBehavior empty)
		{
			var args = new AuthorizationArgs(e.User, Claims.AccessUrl, e.PrimaryKey, "Url");

			args.Schema.Empty = empty;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			return context.Tenant.GetService<IAuthorizationService>().Authorize(context, args);
		}

		private static void Reject(IMiddlewareContext context)
		{
			if (!context.Services.Identity.IsAuthenticated)
				Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else
				Shell.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}
	}
}
