using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Security.AuthorizationProviders
{
	internal class PolicyAuthorizationProvider : IAuthorizationProvider
	{
		public string Id => "Policies";

		public AuthorizationProviderResult Authorize(IMiddlewareContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public AuthorizationProviderResult PreAuthorize(IMiddlewareContext context, AuthorizationArgs e, Dictionary<string, object> state)
		{
			return AuthorizationProviderResult.NotHandled;
		}

		public List<IPermissionSchemaDescriptor> QueryDescriptors(IMiddlewareContext context)
		{
			return new List<IPermissionSchemaDescriptor>();
		}
	}
}
