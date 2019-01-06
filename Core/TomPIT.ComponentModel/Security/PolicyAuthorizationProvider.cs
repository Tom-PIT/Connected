using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class PolicyAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Policies";

		public AuthorizationProviderResult Authorize(IExecutionContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(IExecutionContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IExecutionContext context)
		{
			return new List<IPermissionSchemaDescriptor>();
		}
	}
}
