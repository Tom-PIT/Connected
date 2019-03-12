using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
		public Guid InsertSubscriber()
		{
			var body = FromBody();
			var subscription = body.Required<Guid>("subscription");
			var type = body.Required<SubscriptionResourceType>("type");
			var resourcePrimaryKey = body.Required<string>("resourcePrimaryKey");

			return DataModel.Subscriptions.InsertSubscriber(subscription, type, resourcePrimaryKey);
		}

		//[HttpPost]
		//public Guid InsertSubscribers()
		//{
		//	var body = FromBody();
		//	var subscription = body.Required<Guid>("subscription");
		//	var items = body.Required<JArray>("items");
		//	var subscribers = new List<IRecipient>();

		//	foreach(JObjcet )
		//	var type = body.Required<SubscriptionResourceType>("type");
		//	var resourcePrimaryKey = body.Required<string>("resourcePrimaryKey");

		//	return DataModel.Subscriptions.InsertSubscriber(subscription, type, resourcePrimaryKey);
		//}

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
	}
}
