using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class Subscriber : LongPrimaryKeyRecord, ISubscriber
	{
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public SubscriptionResourceType Type { get; set; }
		public string ResourcePrimaryKey { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Handler = GetGuid("handler");
			Topic = GetString("topic");
			PrimaryKey = GetString("primary_key");
			Type = GetValue("resource_type", SubscriptionResourceType.User);
			ResourcePrimaryKey = GetString("resource_primary_key");
		}
	}
}
