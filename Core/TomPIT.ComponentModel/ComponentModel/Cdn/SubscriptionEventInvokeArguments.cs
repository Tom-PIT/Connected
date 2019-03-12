using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Cdn
{
	public class SubscriptionEventInvokeArguments : EventArguments
	{
		public SubscriptionEventInvokeArguments(IExecutionContext sender, TomPIT.Cdn.ISubscriptionEvent subscriptionEvent, ListItems<IRecipient> recipients) : base(sender)
		{
			SubscriptionEvent = subscriptionEvent;
			Recipient = recipients;
		}

		public TomPIT.Cdn.ISubscriptionEvent SubscriptionEvent { get; }
		public List<IRecipient> Recipient { get; }
	}
}
