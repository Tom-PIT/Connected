using System.Collections.Generic;

namespace TomPIT.Management
{
	public class ManagementSchemaProvider : IManagementSchemaProvider
	{
		public string RootKey { get; protected set; }

		public bool SupportsSchema { get; protected set; }

		public List<IManagementSchemaElement> QuerySchema(string parent)
		{
			return OnQuerySchema(parent);
		}

		protected virtual List<IManagementSchemaElement> OnQuerySchema(string parent)
		{
			return new List<IManagementSchemaElement>();
		}
	}
}
