using System;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Messaging;

namespace TomPIT.SysDb.Sql.Messaging
{
	internal class Topic : LongPrimaryKeyRecord, ITopic
	{
		public string Name { get; set; }
		public Guid ResourceGroup { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Name = GetString("name");
			ResourceGroup = GetGuid("resource_group_token");
		}
	}
}