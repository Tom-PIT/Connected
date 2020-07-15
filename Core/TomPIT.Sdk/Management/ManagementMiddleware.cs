using System.Collections.Generic;
using TomPIT.Annotations;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public abstract class ManagementMiddleware : MiddlewareComponent, IManagementMiddleware
	{
		private List<AuthorizationPolicyAttribute> _authorizationPolicies = null;
		public List<AuthorizationPolicyAttribute> AuthorizationPolicies
		{
			get
			{
				if (_authorizationPolicies == null)
					_authorizationPolicies = OnCreateAuthorizationPolicies();

				return _authorizationPolicies;
			}
		}

		protected virtual List<AuthorizationPolicyAttribute> OnCreateAuthorizationPolicies()
		{
			return new List<AuthorizationPolicyAttribute>();
		}
	}
}
