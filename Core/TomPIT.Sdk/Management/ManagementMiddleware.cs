using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public abstract class ManagementMiddleware : MiddlewareComponent, IManagementMiddleware
	{
		private List<IAuthorizationPolicyDescriptor> _authorizationPolicies = null;
		private List<IConfigurationDescriptor> _configurations = null;
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

		public List<IConfigurationDescriptor> Configuration
		{
			get
			{
				if (_configurations == null)
					_configurations = OnCreateConfiguration();

				return _configurations;
			}
		}

		protected virtual List<IConfigurationDescriptor> OnCreateConfiguration()
		{
			return new List<IConfigurationDescriptor>();
		}
	}
}
