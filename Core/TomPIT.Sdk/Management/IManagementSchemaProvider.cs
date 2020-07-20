using System.Collections.Generic;

namespace TomPIT.Management
{
	public interface IManagementSchemaProvider
	{
		List<IManagementSchemaElement> QuerySchema(string parentId);

		string RootKey { get; }
		bool SupportsSchema { get; }
	}
}
