using System.Collections.Generic;

namespace TomPIT.Annotations
{
	public interface IAuthorizationSchemaProvider
	{
		List<string> QueryClaims();

		List<IAuthorizationSchemaElement> QuerySchema(string parentId);

		string RootKey { get; }
		bool SupportsSchema { get; }
	}
}
