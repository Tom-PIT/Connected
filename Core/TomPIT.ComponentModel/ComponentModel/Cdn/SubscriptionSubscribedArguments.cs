using TomPIT.Services;

namespace TomPIT.ComponentModel.Cdn
{
	public class SubscriptionSubscribedArguments : EventArguments
	{
		public SubscriptionSubscribedArguments(IExecutionContext sender, TomPIT.Cdn.ISubscription subscription) : base(sender)
		{
			Subscription = subscription;
		}

		public TomPIT.Cdn.ISubscription Subscription { get; }
	}
}
