using System;
using System.Net;
using System.Reflection;
using TomPIT.Annotations;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Security
{
	public static class SecurityExtensions
	{
		public static bool MustChangePassword(this IAuthenticationResult result)
		{
			return !result.Success
				&& (result.Reason == AuthenticationResultReason.PasswordExpired || result.Reason == AuthenticationResultReason.NoPassword);
		}

		public static bool AuthorizeUrl(IMiddlewareContext context, string url)
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
				var token = context.Services.Identity.IsAuthenticated ? context.Services.Identity.User.Token : Guid.Empty;
				var ar = AuthorizeUrl(context, new AuthorizationArgs(token, Claims.AccessUrl, path, "Url"), empty);

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

		private static IAuthorizationResult AuthorizeDefaultUrl(IMiddlewareContext context)
		{
			var args = new AuthorizationArgs(context.Services.Identity.IsAuthenticated ? context.Services.Identity.User.Token : Guid.Empty, Claims.DefaultAccessUrl, 0.ToString(), "Default Url");

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

		public static T GetValueFromTarget<T>(this IAuthorizationModel model, string propertyName)
		{
			var properties = model.AuthorizationTarget.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
			/*
			 * First check if attribute is defined on any of the properties. 
			 * This have a higher priority than a property name
			 */
			foreach (var property in properties)
			{
				var attribute = property.FindAttribute<AuthorizationPropertyAttribute>();

				if (attribute != null && string.Compare(attribute.PropertyName, propertyName, true) == 0)
					return Types.Convert<T>(property.GetValue(model.AuthorizationTarget));
			}
			/*
			 * Attribute not defined let's find a property
			 */
			foreach (var property in properties)
			{

				if (string.Compare(property.Name, propertyName, true) == 0)
					return Types.Convert<T>(property.GetValue(model.AuthorizationTarget));
			}
			/*
			 * Property must be defined so we're gonna throw exception
			 */
			throw new ForbiddenException($"{SR.AuthorizationPropertyNotFound} ({propertyName})");
		}
	}
}
