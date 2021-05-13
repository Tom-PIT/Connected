using System;
using TomPIT.Annotations;
using TomPIT.Exceptions;
using TomPIT.Reflection;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareAuthorizationService : MiddlewareComponent, IMiddlewareAuthorizationService
	{
		public MiddlewareAuthorizationService(IMiddlewareContext context) : base(context)
		{
		}

		public void Allow(object claim, object primaryKey, string permissionDescriptor)
		{
			SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Allow);
		}

		public void Allow(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
		{
			SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Allow, schema, evidence);
		}

		public bool Authorize(object claim, object primaryKey, string permissionDescriptor)
		{
			var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

			return Authorize(claim, primaryKey, permissionDescriptor, user);
		}
		public bool Authorize(object claim, object primaryKey, string permissionDescriptor, Guid user)
		{
			return Context.Tenant.GetService<IAuthorizationService>().Authorize(Context, new AuthorizationArgs(user, claim.ToString(), primaryKey.ToString(), permissionDescriptor)).Success;
		}

		public void Deny(object claim, object primaryKey, string permissionDescriptor)
		{
			SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Deny);
		}

		public void Deny(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
		{
			SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Deny, schema, evidence);
		}

		private void SetPermission(object claim, object primaryKey, string permissionDescriptor, PermissionValue value)
		{
			if (!Context.Services.Identity.IsAuthenticated)
				throw new RuntimeException(nameof(MiddlewareAuthorizationService), SR.ErrAuthenticatedUserRequired);

			SetPermission(claim, primaryKey.ToString(), permissionDescriptor, value, "Users", Context.Services.Identity.User.Token.ToString());
		}

		private void SetPermission(object claim, object primaryKey, string permissionDescriptor, PermissionValue value, string schema, string evidence)
		{
			var svc = Context.Tenant.GetService<IAuthorizationService>() as IPermissionService;
			var result = PermissionValue.NotSet;

			while (result != value)
				result = svc.Toggle(claim.ToString(), schema, evidence, primaryKey.ToString(), permissionDescriptor);
		}

		public T CreatePolicy<T>() where T : AuthorizationPolicyAttribute
		{
			return TypeExtensions.CreateInstance<T>(typeof(T));
		}
	}
}
