using System;

namespace TomPIT.Security
{
	public class PermissionEventArgs : EventArgs
	{
		public PermissionEventArgs(Guid evidence, string schema, string claim, string primaryKey)
		{
			Evidence = evidence;
			Schema = schema;
			Claim = claim;
			PrimaryKey = primaryKey;
		}

		public Guid Evidence { get; }
		public string Schema { get; }
		public string Claim { get; set; }
		public string PrimaryKey { get; }
	}
}
