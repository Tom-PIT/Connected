using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;
using TomPIT.Security.PermissionDescriptors;

namespace TomPIT.Security.AuthorizationProviders
{
    internal class UserAuthorizationProvider : IAuthorizationProvider
    {
        public string Id => "Users";

        public async Task<AuthorizationProviderResult> Authorize(IMiddlewareContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
        {
            if (string.Compare(e.User.ToString(), permission.Evidence, true) != 0)
                return await Task.FromResult(AuthorizationProviderResult.NotHandled);

            switch (permission.Value)
            {
                case PermissionValue.NotSet:
                    return AuthorizationProviderResult.NotHandled;
                case PermissionValue.Allow:
                    return AuthorizationProviderResult.Success;
                case PermissionValue.Deny:
                    return AuthorizationProviderResult.Fail;
                default:
                    throw new NotSupportedException();
            }
        }

        public async Task<AuthorizationProviderResult> PreAuthorize(IMiddlewareContext context, AuthorizationArgs e, Dictionary<string, object> state)
        {
            return await Task.FromResult(AuthorizationProviderResult.NotHandled);
        }

        public async Task<List<IPermissionSchemaDescriptor>> QueryDescriptors(IMiddlewareContext context)
        {
            var users = context.Tenant.GetService<IUserService>().Query();
            var r = new List<IPermissionSchemaDescriptor>();

            foreach (var i in users)
            {
                r.Add(new SchemaDescriptor
                {
                    Id = i.Token.ToString(),
                    Title = i.DisplayName(),
                    Description = i.Email
                });
            }

            return await Task.FromResult(r);
        }

    }
}
