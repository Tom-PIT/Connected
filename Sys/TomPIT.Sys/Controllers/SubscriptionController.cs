using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class SubscriptionController : SysController
	{
		[HttpPost]
		public void Enqueue()
		{
			var body = FromBody();

			var microService = body.Required<Guid>("microService");
			var handler = body.Required<Guid>("handler");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");

			DataModel.Subscriptions.Enqueue(microService, handler, topic, primaryKey);
		}

		[HttpPost]
		public bool Exists()
		{
			var body = FromBody();

			var handler = body.Required<Guid>("handler");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");

			return DataModel.Subscriptions.Select(handler, topic, primaryKey) != null;
		}

		[HttpPost]
		public List<ISubscriber> QuerySubscribers()
		{
			var body = FromBody();
			var handler = body.Required<Guid>("handler");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");

			return DataModel.Subscriptions.QuerySubscribers(handler, topic, primaryKey);
		}

		[HttpPost]
		public ISubscriber SelectSubscriber()
		{
			var body = FromBody();
			var handler = body.Required<Guid>("handler");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");
			var type = body.Required<SubscriptionResourceType>("type");
			var resourcePrimaryKey = body.Required<string>("resourcePrimaryKey");

			return DataModel.Subscriptions.SelectSubscriber(handler, topic, primaryKey, type, resourcePrimaryKey);
		}

		[HttpPost]
		public ISubscriber SelectSubscriberByToken()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.Subscriptions.SelectSubscriber(token);
		}

		[HttpPost]
		public ISubscriber SelectSubscriberBySubscription()
		{
			var body = FromBody();
			var subscription = body.Required<Guid>("subscription");
			var type = body.Required<SubscriptionResourceType>("type");
			var resourcePrimaryKey = body.Required<string>("resourcePrimaryKey");

			return DataModel.Subscriptions.SelectSubscriber(subscription, type, resourcePrimaryKey);
		}

		[HttpPost]
		public Guid InsertSubscriber()
		{
			var body = FromBody();
			var subscription = body.Required<Guid>("subscription");
			var type = body.Required<SubscriptionResourceType>("type");
			var resourcePrimaryKey = body.Required<string>("resourcePrimaryKey");

			return DataModel.Subscriptions.InsertSubscriber(subscription, type, resourcePrimaryKey);
		}

		[HttpPost]
		public void InsertSubscribers()
		{
			var body = FromBody();
			var subscription = body.Required<Guid>("subscription");
			var items = body.Required<JArray>("items");
			var subscribers = new List<IRecipient>();

			foreach (JObject recipient in items)
			{
				subscribers.Add(new Recipient
				{
					Type = recipient.Required<SubscriptionResourceType>("type"),
					ResourcePrimaryKey = recipient.Required<string>("resourcePrimaryKey")
				});
			}

			DataModel.Subscriptions.InsertSubscribers(subscription, subscribers);
		}

		[HttpPost]
		public void DeleteSubscriber()
		{
			var body = FromBody();
			var subscription = body.Required<Guid>("subscription");
			var type = body.Required<SubscriptionResourceType>("type");
			var pk = body.Required<string>("resourcePrimaryKey");

			DataModel.Subscriptions.DeleteSubscriber(subscription, type, pk);
		}

		[HttpPost]
		public Guid EnqueueEvent()
		{
			var body = FromBody();
			var microService = body.Required<Guid>("microService");
			var handler = body.Required<Guid>("handler");
			var name = body.Required<string>("name");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");
			var arguments = body.Optional("arguments", string.Empty);

			return DataModel.Subscriptions.InsertSubscriptionEvent(microService, handler, name, topic, primaryKey, arguments);
		}

		[HttpPost]
		public ISubscription SelectSubscription()
		{
			var body = FromBody();
			var token = body.Required<Guid>("token");

			return DataModel.Subscriptions.Select(token);
		}

		[HttpPost]
		public ISubscription SelectSubscriptionByConfiguration()
		{
			var body = FromBody();
			var handler = body.Required<Guid>("handler");
			var topic = body.Optional("topic", string.Empty);
			var primaryKey = body.Required<string>("primaryKey");

			return DataModel.Subscriptions.Select(handler, topic, primaryKey);
		}
	}
}
