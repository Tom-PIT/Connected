using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class Role : PrimaryKeyRecord, IRole
	{
		public Guid Token { get; set; }
		public string Name { get; set; }
		public RoleBehavior Behavior { get; set; }
		public RoleVisibility Visibility { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Token = GetGuid("token");
			Name = GetString("name");
			Behavior = RoleBehavior.Explicit;
			Visibility = RoleVisibility.Visible;
		}
	}
}
