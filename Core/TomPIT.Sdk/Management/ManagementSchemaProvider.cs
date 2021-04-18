using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Management
{
	public class ManagementSchemaProvider : MiddlewareObject, IManagementSchemaProvider
	{
		public string RootKey { get; protected set; }

		public bool SupportsSchema { get; protected set; }

		public List<IManagementSchemaElement> QuerySchema(string parentId, string parentKind, SchemaElementType parentType)
		{
			return OnQuerySchema(parentId, parentKind, parentType);
		}

		protected virtual List<IManagementSchemaElement> OnQuerySchema(string parentId, string parentKind, SchemaElementType parentType)
		{
			return new List<IManagementSchemaElement>();
		}
	}
}
