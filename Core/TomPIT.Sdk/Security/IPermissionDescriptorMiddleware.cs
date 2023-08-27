using System.Collections.Generic;
using System.Threading.Tasks;
using TomPIT.Middleware;

namespace TomPIT.Security
{
    public interface IPermissionDescriptorMiddleware : IMiddlewareObject
    {
        Task<AuthorizationProviderResult> Authorize(IPermission permission, AuthorizationArgs e, Dictionary<string, object> state);
    }
}
