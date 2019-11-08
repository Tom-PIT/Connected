using System.Collections.Generic;
using TomPIT.ComponentModel.Data;

namespace TomPIT.Data.DataProviders.Design
{
	public interface ISchemaBrowser
	{
		List<string> QuerySchemaGroups(IConnectionConfiguration repository);

		List<IGroupObject> QueryGroupObjects(IConnectionConfiguration repository);
		List<IGroupObject> QueryGroupObjects(IConnectionConfiguration repository, string schemaGroup);

		List<ISchemaField> QuerySchema(IConnectionConfiguration repository, string schemaGroup, string groupObject);
		List<ISchemaParameter> QueryParameters(IConnectionConfiguration repository, string schemaGroup, string groupObject);
		List<ISchemaParameter> QueryParameters(IConnectionConfiguration repository, string groupObject);

		ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject);
	}
}