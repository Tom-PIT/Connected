using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class Subscriber : LongPrimaryKeyRecord, ISubscriber
	{
		public Guid Subscription { get; set; }
		public SubscriptionResourceType Type { get; set; }
		public string ResourcePrimaryKey { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Subscription = GetGuid("subscription_token");
			Type = GetValue("resource_type", SubscriptionResourceType.User);
			ResourcePrimaryKey = GetString("resource_primary_key");
		}
	}
}
