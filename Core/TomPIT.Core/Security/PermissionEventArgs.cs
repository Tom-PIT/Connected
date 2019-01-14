using System;

namespace TomPIT.Security
{
	public class PermissionEventArgs : EventArgs
	{
		public PermissionEventArgs(Guid resourceGroup, Guid evidence, string schema, string claim, string primaryKey)
		{
			Evidence = evidence;
			Schema = schema;
			Claim = claim;
			PrimaryKey = primaryKey;
			ResourceGroup = resourceGroup;
		}

		public Guid ResourceGroup { get; }
		public Guid Evidence { get; }
		public string Schema { get; }
		public string Claim { get; set; }
		public string PrimaryKey { get; }
	}
}
