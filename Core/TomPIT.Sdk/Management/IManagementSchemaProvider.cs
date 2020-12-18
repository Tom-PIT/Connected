using System.Collections.Generic;

namespace TomPIT.Management
{
	public interface IManagementSchemaProvider
	{
		List<IManagementSchemaElement> QuerySchema(string parentId, string parentKind, SchemaElementType parentType);

		string RootKey { get; }
		bool SupportsSchema { get; }
	}
}
