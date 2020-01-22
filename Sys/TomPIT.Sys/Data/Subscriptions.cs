using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class Subscriptions
	{
		private const string Queue = "subscription";
		private const string EventQueue = "subscriptionEvent";

		public ISubscription Select(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.Select(token);
		}

		public ISubscription Select(Guid handler, string topic, string primaryKey)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.Select(handler, topic, primaryKey);
		}

		public void Delete(Guid handler, string topic, string primaryKey)
		{
			var subscription = Select(handler, topic, primaryKey);

			if (subscription == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.Delete(subscription);
		}

		public List<ISubscriber> QuerySubscribers(Guid handler, string topic, string primaryKey)
		{
			var subscription = Select(handler, topic, primaryKey);

			if (subscription == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.QuerySubscribers(subscription);
		}

		public void Enqueue(Guid microService, Guid handler, string topic, string primaryKey)
		{
			var id = Guid.NewGuid();

			var ms = microService == Guid.Empty
				? null
				: DataModel.MicroServices.Select(microService);

			if (microService != Guid.Empty && ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var message = new JObject
			{
				{ "id",id}
			};

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.Insert(id, handler, topic, primaryKey);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, SysDb.Messaging.QueueScope.System);
		}

		public ISubscriber SelectSubscriber(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectSubscriber(token);
		}

		public ISubscriber SelectSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var sub = Select(subscription);

			if (sub == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectSubscriber(sub, type, resourcePrimaryKey);
		}

		public ISubscriber SelectSubscriber(Guid handler, string topic, string primaryKey, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var subscription = Select(handler, topic, primaryKey);

			if (subscription == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectSubscriber(subscription, type, resourcePrimaryKey);
		}

		public void InsertSubscribers(Guid subscription, List<IRecipient> subscribers)
		{
			var sub = Select(subscription);

			if (sub == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.InsertSubscribers(sub, subscribers);
		}

		public Guid InsertSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var sub = Select(subscription);

			if (subscription == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			var id = Guid.NewGuid();

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.InsertSubscriber(sub, id, type, resourcePrimaryKey);

			return id;
		}

		public void DeleteSubscriber(Guid subscription, SubscriptionResourceType type, string resourcePrimaryKey)
		{
			var sub = Select(subscription);

			if (sub == null)
				throw new SysException(SR.ErrSubscriptionNotFound);

			var subscriber = Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectSubscriber(sub, type, resourcePrimaryKey);

			if (subscriber == null)
				throw new SysException(SR.ErrSubscriberNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.DeleteSubscriber(subscriber);
		}

		public Guid InsertSubscriptionEvent(Guid microService, Guid handler, string name, string topic, string primaryKey, string arguments)
		{
			var sub = Select(handler, topic, primaryKey);

			if (sub == null)
				throw new SysException($"{SR.ErrSubscriptionNotFoundForPrimaryKey} ({primaryKey})");

			var id = Guid.NewGuid();
			var ms = microService == Guid.Empty
				? null
				: DataModel.MicroServices.Select(microService);

			var message = new JObject
			{
				{ "id",id}
			};


			if (microService != Guid.Empty && ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.InsertEvent(sub, id, name, DateTime.UtcNow, arguments);
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Enqueue(EventQueue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, SysDb.Messaging.QueueScope.System);

			return id;
		}

		public List<ISubscriptionEvent> QueryEvents()
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.QueryEvents();
		}

		public ISubscriptionEvent SelectEvent(Guid token)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectEvent(token);
		}

		public List<IQueueMessage> Dequeue(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(Queue, count);
		}

		public List<IQueueMessage> DequeueEvents(int count)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.DequeueSystem(EventQueue, count);
		}

		public void Ping(Guid popReceipt)
		{
			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Ping(popReceipt, TimeSpan.FromSeconds(5));
		}

		public void Complete(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
		}

		public void CompleteEvent(Guid popReceipt)
		{
			var m = Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Select(popReceipt);

			if (m == null)
				return;

			Shell.GetService<IDatabaseService>().Proxy.Messaging.Queue.Delete(popReceipt);
			var ev = ResolveEvent(m);

			if (ev != null)
				Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.DeleteEvent(ev);
		}

		private ISubscriptionEvent ResolveEvent(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectEvent(id);
		}
	}
}
