using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public interface IManagementMiddleware : IMiddlewareComponent
	{
		List<AuthorizationPolicyAttribute> AuthorizationPolicies { get; }
	}
}
