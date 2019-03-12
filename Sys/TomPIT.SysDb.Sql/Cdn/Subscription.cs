using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class Subscription : LongPrimaryKeyRecord, ISubscription
	{
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Handler = GetGuid("handler");
			Topic = GetString("topic");
			PrimaryKey = GetString("primary_key");
		}
	}
}
