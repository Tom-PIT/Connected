using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.Security
{
	internal class EnvironmentUnitAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Environment";

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
