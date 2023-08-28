using System;
using System.Threading.Tasks;
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

        [Obsolete("Please use async method")]
        public void Allow(object claim, object primaryKey, string permissionDescriptor)
        {
            AsyncUtils.RunSync(() => AllowAsync(claim, primaryKey, permissionDescriptor));
        }
        [Obsolete("Please use async method")]
        public void Allow(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
        {
            AsyncUtils.RunSync(() => AllowAsync(claim, primaryKey, permissionDescriptor, schema, evidence));
        }
        [Obsolete("Please use async method")]
        public bool Authorize(object claim, object primaryKey, string permissionDescriptor)
        {
            return AsyncUtils.RunSync(() => AuthorizeAsync(claim, primaryKey, permissionDescriptor));
        }
        [Obsolete("Please use async method")]
        public bool Authorize(object claim, object primaryKey, string permissionDescriptor, Guid user)
        {
            return AsyncUtils.RunSync(() => AuthorizeAsync(claim, primaryKey, permissionDescriptor, user));
        }
        [Obsolete("Please use async method")]
        public void Deny(object claim, object primaryKey, string permissionDescriptor)
        {
            AsyncUtils.RunSync(() => DenyAsync(claim, primaryKey, permissionDescriptor));
        }
        [Obsolete("Please use async method")]
        public void Deny(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
        {
            AsyncUtils.RunSync(() => AllowAsync(claim, primaryKey, permissionDescriptor, schema, evidence));
        }

        public async Task AllowAsync(object claim, object primaryKey, string permissionDescriptor)
        {
            await SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Allow);
        }

        public async Task AllowAsync(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
        {
            await SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Allow, schema, evidence);
        }

        public async Task<bool> AuthorizeAsync(object claim, object primaryKey, string permissionDescriptor)
        {
            var user = Context.Services.Identity.IsAuthenticated ? Context.Services.Identity.User.Token : Guid.Empty;

            return await AuthorizeAsync(claim, primaryKey, permissionDescriptor, user);
        }

        public async Task<bool> AuthorizeAsync(object claim, object primaryKey, string permissionDescriptor, Guid user)
        {
            return (await Context.Tenant.GetService<IAuthorizationService>().AuthorizeAsync(Context, new AuthorizationArgs(user, claim.ToString(), primaryKey.ToString(), permissionDescriptor))).Success;
        }

        public async Task DenyAsync(object claim, object primaryKey, string permissionDescriptor)
        {
            await SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Deny);
        }

        public async Task DenyAsync(object claim, object primaryKey, string permissionDescriptor, string schema, string evidence)
        {
            await SetPermission(claim, primaryKey, permissionDescriptor, PermissionValue.Deny, schema, evidence);
        }

        private async Task SetPermission(object claim, object primaryKey, string permissionDescriptor, PermissionValue value)
        {
            if (!Context.Services.Identity.IsAuthenticated)
                throw new RuntimeException(nameof(MiddlewareAuthorizationService), SR.ErrAuthenticatedUserRequired);

            await SetPermission(claim, primaryKey.ToString(), permissionDescriptor, value, "Users", Context.Services.Identity.User.Token.ToString());
        }

        private async Task SetPermission(object claim, object primaryKey, string permissionDescriptor, PermissionValue value, string schema, string evidence)
        {
            var svc = Context.Tenant.GetService<IAuthorizationService>() as IPermissionService;
            var result = PermissionValue.NotSet;

            while (result != value)
                result = await svc.ToggleAsync(claim.ToString(), schema, evidence, primaryKey.ToString(), permissionDescriptor);
        }

        public T CreatePolicy<T>() where T : AuthorizationPolicyAttribute
        {
            return TypeExtensions.CreateInstance<T>(typeof(T));
        }
    }
}
