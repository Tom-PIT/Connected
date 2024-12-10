using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Security
{
	public enum AuthorizationProviderResult
	{
		Success = 1,
		Fail = 2,
		NotHandled = 3
	}

	public interface IAuthorizationProvider
	{
		string Id { get; }

		AuthorizationProviderResult PreAuthorize(IMiddlewareContext context, AuthorizationArgs e, Dictionary<string, object> state);
		AuthorizationProviderResult Authorize(IMiddlewareContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state);
		List<IPermissionSchemaDescriptor> QueryDescriptors(IMiddlewareContext context);
	}
}
