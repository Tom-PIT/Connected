using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public abstract class ManagementMiddleware : MiddlewareComponent, IManagementMiddleware
	{
		private List<IAuthorizationPolicyDescriptor> _authorizationPolicies = null;
		public List<IAuthorizationPolicyDescriptor> AuthorizationPolicies
		{
			get
			{
				if (_authorizationPolicies == null)
					_authorizationPolicies = OnCreateAuthorizationPolicies();

				return _authorizationPolicies;
			}
		}

		protected virtual List<IAuthorizationPolicyDescriptor> OnCreateAuthorizationPolicies()
		{
			return new List<IAuthorizationPolicyDescriptor>();
		}
	}
}
