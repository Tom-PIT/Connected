using System;
using TomPIT.Annotations;

namespace TomPIT.Management
{
	public interface IAuthorizationPolicyDescriptor
	{
		[Obsolete]
		AuthorizationPolicyAttribute Policy { get; }
		IAuthorizationSchemaProvider SchemaProvider { get; }
	}
}
