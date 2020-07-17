using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public interface IManagementMiddleware : IMiddlewareComponent
	{
		List<IAuthorizationPolicyDescriptor> AuthorizationPolicies { get; }
	}
}
