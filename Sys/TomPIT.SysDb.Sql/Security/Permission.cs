using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class Permission : LongPrimaryKeyRecord, IPermission
	{
		public Guid Evidence { get; set; }
		public string Schema { get; set; }
		public string Claim { get; set; }
		public string Descriptor { get; set; }
		public string PrimaryKey { get; set; }
		public PermissionValue Value { get; set; }
		public Guid ResourceGroup { get; set; }
		public string Component { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Evidence = GetGuid("evidence");
			Schema = GetString("schema");
			Claim = GetString("claim");
			Descriptor = GetString("descriptor");
			PrimaryKey = GetString("primary_key");
			Value = GetValue("value", PermissionValue.NotSet);
			Component = GetString("component");
			ResourceGroup = GetGuid("resource_group_token");
		}
	}
}
