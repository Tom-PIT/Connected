using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Cdn
{
	public class SubscriptionSubscribedArguments : DataModelContext
	{
		public SubscriptionSubscribedArguments(IExecutionContext sender, TomPIT.Cdn.ISubscription subscription) : base(sender)
		{
			Subscription = subscription;
		}

		public TomPIT.Cdn.ISubscription Subscription { get; }
	}
}
