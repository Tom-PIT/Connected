using System.Collections.Generic;
using TomPIT.Services;

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

		AuthorizationProviderResult PreAuthorize(IExecutionContext context, AuthorizationArgs e, Dictionary<string, object> state);
		AuthorizationProviderResult Authorize(IExecutionContext context, IPermission permission, AuthorizationArgs e, Dictionary<string, object> state);
		List<IPermissionSchemaDescriptor> QueryDescriptors(IExecutionContext context);
	}
}
