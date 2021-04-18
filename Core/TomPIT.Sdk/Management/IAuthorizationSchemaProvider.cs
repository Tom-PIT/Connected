using System.Collections.Generic;

namespace TomPIT.Management
{
	public interface IAuthorizationSchemaProvider : IManagementSchemaProvider
	{
		List<string> QueryClaims();
	}
}
