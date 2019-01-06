using System.Collections.Generic;
using TomPIT.Runtime;

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

		AuthorizationProviderResult PreAuthorize(IApplicationContext context, AuthorizationArgs e, Dictionary<string, object> state);
		AuthorizationProviderResult Authorize(IApplicationContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state);
		List<IPermissionSchemaDescriptor> QueryDescriptors(IApplicationContext context);
	}
}
