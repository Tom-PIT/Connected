using System;
using TomPIT.Cdn;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionEventDescriptor : ISubscriptionEvent
	{
		public Guid Subscription { get; set; }
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
		public string Name { get; set; }
		public DateTime Created { get; set; }
		public Guid Token { get; set; }
		public string Arguments { get; set; }
	}
}
