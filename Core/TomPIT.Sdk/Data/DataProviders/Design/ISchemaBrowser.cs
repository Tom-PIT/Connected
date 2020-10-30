using System.Collections.Generic;
using TomPIT.ComponentModel.Data;
using TomPIT.Middleware;

namespace TomPIT.Data.DataProviders.Design
{
	public enum DataOperation
	{
		NotSet = 0,
		Read = 1,
		Write = 2
	}
	public interface ISchemaBrowser : IMiddlewareObject
	{
		List<string> QuerySchemaGroups(IConnectionConfiguration configuration);

		List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration);
		List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration, string schemaGroup);

		List<ISchemaField> QuerySchema(IConnectionConfiguration configuration, string schemaGroup, string groupObject);
		List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string schemaGroup, string groupObject, DataOperation operation);
		List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string groupObject, DataOperation operation);

		ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject);
	}
}