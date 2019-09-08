using System;
using TomPIT.Cdn;

namespace TomPIT.Worker.Subscriptions
{
	internal class Subscriber : ISubscriber
	{
		public Guid Subscription { get; set; }
		public SubscriptionResourceType Type { get; set; }
		public string ResourcePrimaryKey { get; set; }
	}
}
