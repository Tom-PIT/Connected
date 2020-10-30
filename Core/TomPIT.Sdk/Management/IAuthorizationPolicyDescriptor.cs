using TomPIT.Annotations;

namespace TomPIT.Management
{
	public interface IAuthorizationPolicyDescriptor
	{
		AuthorizationPolicyAttribute Policy { get; }
		IAuthorizationSchemaProvider SchemaProvider { get; }
	}
}
