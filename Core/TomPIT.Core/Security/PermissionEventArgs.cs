using System;

namespace TomPIT.Security
{
	public class PermissionEventArgs : EventArgs
	{
		public PermissionEventArgs()
		{

		}
		public PermissionEventArgs(Guid resourceGroup, Guid evidence, string schema, string claim, string primaryKey)
		{
			Evidence = evidence;
			Schema = schema;
			Claim = claim;
			PrimaryKey = primaryKey;
			ResourceGroup = resourceGroup;
		}

		public Guid ResourceGroup { get; set; }
		public Guid Evidence { get; set; }
		public string Schema { get; set; }
		public string Claim { get; set; }
		public string PrimaryKey { get; set; }
	}
}
