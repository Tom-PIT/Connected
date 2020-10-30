using TomPIT.Annotations;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public class AuthorizationPolicyDescriptor : MiddlewareObject, IAuthorizationPolicyDescriptor
	{
		public AuthorizationPolicyAttribute Policy { get; set; }

		public IAuthorizationSchemaProvider SchemaProvider { get; set; }
	}
}
