using System.Collections.Generic;
using TomPIT.ComponentModel.Data;
using TomPIT.Middleware;

namespace TomPIT.Data.DataProviders.Design
{
	public abstract class SchemaBrowser : MiddlewareObject, ISchemaBrowser
	{
		public abstract ICommandDescriptor CreateCommandDescriptor(string schemaGroup, string groupObject);
		public abstract List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration);
		public abstract List<IGroupObject> QueryGroupObjects(IConnectionConfiguration configuration, string schemaGroup);
		public abstract List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string schemaGroup, string groupObject, DataOperation operation);
		public abstract List<ISchemaParameter> QueryParameters(IConnectionConfiguration configuration, string groupObject, DataOperation operation);
		public abstract List<ISchemaField> QuerySchema(IConnectionConfiguration configuration, string schemaGroup, string groupObject);
		public abstract List<string> QuerySchemaGroups(IConnectionConfiguration configuration);

		protected IConnectionString ResolveConnectionString(IConnectionConfiguration configuration)
		{
			return configuration.ResolveConnectionString(Context, ConnectionStringContext.Elevated);
		}
	}
}
