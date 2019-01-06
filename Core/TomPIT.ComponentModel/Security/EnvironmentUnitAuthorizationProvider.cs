using System.Collections.Generic;
using TomPIT.Runtime;

namespace TomPIT.Security
{
	internal class EnvironmentUnitAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Environment";

		public AuthorizationProviderResult Authorize(IApplicationContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(IApplicationContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IApplicationContext context)
		{
			return new List<IPermissionSchemaDescriptor>();
		}
	}
}
