using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Cdn
{
	public class SubscriptionEventInvokeArguments : DataModelContext
	{
		public SubscriptionEventInvokeArguments(IExecutionContext sender, TomPIT.Cdn.ISubscriptionEvent subscriptionEvent, ListItems<IRecipient> recipients) : base(sender)
		{
			SubscriptionEvent = subscriptionEvent;
			Recipients = recipients;
		}

		public TomPIT.Cdn.ISubscriptionEvent SubscriptionEvent { get; }
		public List<IRecipient> Recipients { get; }
	}
}
