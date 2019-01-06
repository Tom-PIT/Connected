using System;
using TomPIT.Data.Sql;
using TomPIT.Security;

namespace TomPIT.SysDb.Sql.Security
{
	internal class Membership : LongPrimaryKeyRecord, IMembership
	{
		public Guid User { get; set; }
		public Guid Role { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			User = GetGuid("user_token");
			Role = GetGuid("role");
		}
	}
}
