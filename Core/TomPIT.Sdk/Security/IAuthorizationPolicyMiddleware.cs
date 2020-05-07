using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public interface IAuthorizationPolicyMiddleware : IMiddlewareComponent
	{
		List<IAuthorizationPolicyClaim> Claims { get; }

		List<IAuthorizationPolicyEntity> QueryEntities(IAuthorizationPolicyClaim claim);

		void Authorize(object instance, string policy);
	}
}
