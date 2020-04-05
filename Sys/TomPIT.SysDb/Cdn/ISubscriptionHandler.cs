using System;
using System.Collections.Generic;
using TomPIT.Cdn;

namespace TomPIT.SysDb.Cdn
{
	public interface ISubscriptionHandler
	{
		void Insert(Guid token, Guid handler, string topic, string primaryKey);
		ISubscription Select(Guid token);
		ISubscription Select(Guid handler, string topic, string primaryKey);
		void Delete(ISubscription subscription);
		void Delete(Guid handler);

		List<ISubscriber> QuerySubscribers(ISubscription subscription);
		ISubscriber SelectSubscriber(ISubscription subscription, SubscriptionResourceType type, string resourcePrimaryKey);
		ISubscriber SelectSubscriber(Guid token);

		void InsertSubscriber(ISubscription subscription, Guid token, SubscriptionResourceType type, string resourcePrimaryKey);
		void DeleteSubscriber(ISubscriber subscriber);

		void InsertEvent(ISubscription subscription, Guid token, string name, DateTime created, string arguments);
		List<ISubscriptionEvent> QueryEvents();
		ISubscriptionEvent SelectEvent(Guid token);
		void DeleteEvent(ISubscriptionEvent d);
		void InsertSubscribers(ISubscription subscription, List<IRecipient> subscribers);
	}
}
