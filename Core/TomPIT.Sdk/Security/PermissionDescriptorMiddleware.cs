using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Security
{
    public abstract class PermissionDescriptorMiddleware : MiddlewareObject, IPermissionDescriptorMiddleware
    {
        public async Task<AuthorizationProviderResult> Authorize(IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
        {
            State = state;
            Value = permission.Value;
            PrimaryKey = e.PrimaryKey;
            User = e.User;
            Claim = e.Claim;

            return await OnAuthorize();
        }

        protected string Claim { get; private set; }
        protected Guid User { get; private set; }
        protected PermissionValue Value { get; private set; }
        protected string PrimaryKey { get; private set; }
        protected Dictionary<string, object> State { get; private set; }

        protected virtual async Task<AuthorizationProviderResult> OnAuthorize()
        {
            return await Task.FromResult(AuthorizationProviderResult.NotHandled);
        }
    }
}
