using System;
using System.Net;
using TomPIT.ComponentModel;
using TomPIT.Security;
using TomPIT.Services;

namespace TomPIT
{
	public static class SecurityExtensions
	{
		public static bool AuthorizeUrl(IExecutionContext context, string url)
		{
			if (string.IsNullOrWhiteSpace(url))
				return false;

			var tokens = url.Split('/');
			var path = string.Empty;
			var permissionCounter = 0;

			var defaultAr = AuthorizeDefaultUrl(context);

			if (!defaultAr.Success)
			{
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
				var ar = AuthorizeUrl(context, new AuthorizationArgs(context.GetAuthenticatedUserToken(), Claims.AccessUrl, path), empty);

				if (ar.Success)
					permissionCounter += ar.PermissionCount;
				else
				{
					if (empty == EmptyBehavior.Deny && ar.Reason == AuthorizationResultReason.Empty && permissionCounter > 0)
						return true;
					else
					{
						Reject(context);
						return false;
					}
				}
			}

			return true;
		}

		private static IAuthorizationResult AuthorizeDefaultUrl(IExecutionContext context)
		{
			var args = new AuthorizationArgs(context.GetAuthenticatedUserToken(), Claims.DefaultAccessUrl, 0.ToString(), Guid.Empty);

			args.Schema.Empty = EmptyBehavior.Alow;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			return context.Connection().GetService<IAuthorizationService>().Authorize(context, args);
		}

		private static IAuthorizationResult AuthorizeUrl(IExecutionContext context, AuthorizationArgs e, EmptyBehavior empty)
		{
			var args = new AuthorizationArgs(e.User, Claims.AccessUrl, e.PrimaryKey, Guid.Empty);

			args.Schema.Empty = empty;
			args.Schema.Level = AuthorizationLevel.Pessimistic;

			return context.Connection().GetService<IAuthorizationService>().Authorize(context, args);
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
