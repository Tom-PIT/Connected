using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class SubscriptionEvent : LongPrimaryKeyRecord, ISubscriptionEvent
	{
		public Guid Subscription { get; set; }
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public Guid Token { get; set; }
		public string Arguments { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Subscription = GetGuid("subscription_token");
			Name = GetString("name");
			Created = GetDate("created");
			Token = GetGuid("token");
			Arguments = GetString("arguments");
			Handler = GetGuid("handler");
			Topic = GetString("topic");
			PrimaryKey = GetString("primary_key");
		}
	}
}
