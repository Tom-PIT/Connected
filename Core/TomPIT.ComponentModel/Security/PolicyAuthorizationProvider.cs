using System.Collections.Generic;
using TomPIT.Runtime;

namespace TomPIT.Security
{
	internal class PolicyAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Policies";

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
