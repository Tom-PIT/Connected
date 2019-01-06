using System.Collections.Generic;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Data.DataProviders
{
	public interface ISchemaBrowser
	{
		List<string> QuerySchemaGroups(IConnection repository);

		List<string> QueryGroupObjects(IConnection repository, string schemaGroup);

		List<ISchemaField> QuerySchema(IConnection repository, string schemaGroup, string groupObject);
		List<ISchemaParameter> QueryParameters(IConnection repository, string schemaGroup, string groupObject);

		ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject);
	}
}