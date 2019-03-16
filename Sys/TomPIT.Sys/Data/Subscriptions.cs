using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Api.Storage;
using TomPIT.Cdn;
using TomPIT.Storage;
using TomPIT.Sys.Api.Database;
using TomPIT.SysDb.Environment;

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

			var rg = DataModel.ResourceGroups.Select(ms == null ? Guid.Empty : ms.ResourceGroup);
			var sp = Shell.GetService<IStorageProviderService>().Select(rg.StorageProvider);

			var message = new JObject
			{
				{ "id",id}
			};

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.Insert(id, handler, topic, primaryKey);
			sp.Queue.Enqueue(rg, Queue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);
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

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.InsertSubscriber(sub, type, resourcePrimaryKey);

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
				throw new SysException(SR.ErrSubscriptionEventNotFound);

			var id = Guid.NewGuid();
			var ms = microService == Guid.Empty
				? null
				: DataModel.MicroServices.Select(microService);

			var rg = DataModel.ResourceGroups.Select(ms == null ? Guid.Empty : ms.ResourceGroup);
			var sp = Shell.GetService<IStorageProviderService>().Select(rg.StorageProvider);

			var message = new JObject
			{
				{ "id",id}
			};


			if (microService != Guid.Empty && ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.InsertEvent(sub, id, name, DateTime.UtcNow, arguments);
			sp.Queue.Enqueue(rg, EventQueue, JsonConvert.SerializeObject(message), TimeSpan.FromDays(2), TimeSpan.Zero, QueueScope.System);

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

		public List<IClientQueueMessage> Dequeue(IServerResourceGroup resourceGroup, int count)
		{
			var provider = Shell.GetService<IStorageProviderService>().Select(resourceGroup.StorageProvider);

			if (provider == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrStorageProviderNotRegistered, resourceGroup.StorageProvider.ToString()));

			return provider.Queue.DequeueSystem(resourceGroup, Queue, count).ToClientQueueMessage(resourceGroup.Token);
		}

		public List<IClientQueueMessage> DequeueEvents(IServerResourceGroup resourceGroup, int count)
		{
			var provider = Shell.GetService<IStorageProviderService>().Select(resourceGroup.StorageProvider);

			if (provider == null)
				throw new SysException(string.Format("{0} ({1})", SR.ErrStorageProviderNotRegistered, resourceGroup.StorageProvider.ToString()));

			return provider.Queue.DequeueSystem(resourceGroup, EventQueue, count).ToClientQueueMessage(resourceGroup.Token);
		}

		public void Ping(Guid resourceGroup, Guid popReceipt)
		{
			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			sp.Queue.Ping(res, popReceipt, TimeSpan.FromSeconds(5));
		}

		public void Complete(Guid resourceGroup, Guid popReceipt)
		{
			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = sp.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			sp.Queue.Delete(res, popReceipt);
		}

		public void CompleteEvent(Guid resourceGroup, Guid popReceipt)
		{
			var sp = Shell.GetService<IStorageProviderService>().Resolve(resourceGroup);
			var res = DataModel.ResourceGroups.Select(resourceGroup);

			if (res == null)
				throw new SysException(SR.ErrResourceGroupNotFound);

			var m = sp.Queue.Select(res, popReceipt);

			if (m == null)
				return;

			sp.Queue.Delete(res, popReceipt);
		}

		private ISubscriptionEvent ResolveEvent(IQueueMessage message)
		{
			var d = JsonConvert.DeserializeObject(message.Message) as JObject;

			var id = d.Required<Guid>("id");

			return Shell.GetService<IDatabaseService>().Proxy.Cdn.Subscription.SelectEvent(id);
		}
	}
}
