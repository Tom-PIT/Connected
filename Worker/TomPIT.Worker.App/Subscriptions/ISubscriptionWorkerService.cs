using System;
using System.Collections.Generic;
using TomPIT.Cdn;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal interface ISubscriptionWorkerService
	{
		void CompleteSubscription(Guid popReceipt);
		void CompleteEvent(Guid popReceipt);
		ISubscriptionEvent SelectEvent(Guid token);
		void InsertSubscribers(Guid token, List<IRecipient> recipients);
		void PingSubscription(Guid popReceipt);
		void PingSubscriptionEvent(Guid popReceipt);
		List<IRecipient> QueryRecipients(Guid subscription, string primaryKey, string topic);
		List<IQueueMessage> DequeueSubscriptions(string resourceGroup, int count);
		List<IQueueMessage> DequeueSubscriptionEvents(string resourceGroup, int count);
	}
}
