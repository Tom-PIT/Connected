using System;
using TomPIT.Cdn;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionDescriptor : ISubscription
	{
		public Guid Handler { get; set; }
		public string Topic { get; set; }
		public string PrimaryKey { get; set; }
	}
}
