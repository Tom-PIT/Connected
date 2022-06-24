using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Cdn
{
	internal class Subscriber : LongPrimaryKeyRecord, ISubscriber
	{
		public Guid Subscription { get; set; }
		public SubscriptionResourceType Type { get; set; }
		public string ResourcePrimaryKey { get; set; }

		public Guid Token { get; set; }
		public List<string> Tags { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Subscription = GetGuid("subscription_token");
			Type = GetValue("resource_type", SubscriptionResourceType.User);
			ResourcePrimaryKey = GetString("resource_primary_key");
			Token = GetGuid("token");

			var tagString = GetString("tags");

			if (!string.IsNullOrWhiteSpace(tagString))
			{
				var tokens = tagString.Split(',');
				Tags = new List<string>();

				foreach (var token in tokens)
					Tags.Add(token);
			}
		}
	}
}
