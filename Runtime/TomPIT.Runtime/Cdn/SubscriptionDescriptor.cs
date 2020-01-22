using System;

namespace TomPIT.Cdn
{
	internal class SubscriptionDescriptor : ISubscription
	{
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public Guid Token { get; set; }
	}
}
