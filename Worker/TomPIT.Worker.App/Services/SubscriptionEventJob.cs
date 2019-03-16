using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionEventJob : DispatcherJob<IClientQueueMessage>
	{
		public SubscriptionEventJob(Dispatcher<IClientQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IClientQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "CompleteEvent");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt },
				{"resourceGroup", item.ResourceGroup }
			};

			Instance.Connection.Post(url, d);
		}

		private void Invoke(IClientQueueMessage item, JObject message)
		{
			var id = message.Required<Guid>("id");
			var subscriptionEvent = SelectEvent(id);

			if (subscriptionEvent == null)
				return;

			if (!(Instance.Connection.GetService<IComponentService>().SelectConfiguration(subscriptionEvent.Handler) is TomPIT.ComponentModel.Cdn.ISubscription config))
				return;

			var eventConfig = config.Events.FirstOrDefault(f => string.Compare(f.Name, subscriptionEvent.Name, true) == 0);

			if (eventConfig == null)
				return;

			var ms = config.MicroService(Instance.Connection);
			var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, Instance.GetService<IMicroServiceService>().Select(ms), string.Empty);
			var subscribers = QuerySubscribers(subscriptionEvent);
			var invokeArgs = new SubscriptionEventInvokeArguments(ctx, subscriptionEvent, subscribers.ToList<IRecipient>());

			Instance.GetService<ICompilerService>().Execute(ms, eventConfig.Invoke, this, invokeArgs);
		}

		private Cdn.ISubscriptionEvent SelectEvent(Guid token)
		{
			var u = Instance.Connection.CreateUrl("SubscriptionManagement", "SelectEvent");
			var e = new JObject
			{
				{"token",  token}
			};

			return Instance.Connection.Post<SubscriptionEventDescriptor>(u, e);
		}

		private ListItems<ISubscriber> QuerySubscribers(Cdn.ISubscriptionEvent ev)
		{
			var u = Instance.Connection.CreateUrl("Subscription", "QuerySubscribers");
			var e = new JObject
			{
				{"handler",  ev.Handler},
				{"primaryKey",  ev.PrimaryKey}
			};

			if (!string.IsNullOrWhiteSpace(ev.Topic))
				e.Add("topic", ev.Topic);

			return Instance.Connection.Post<List<Subscriber>>(u, e).ToList<ISubscriber>();
		}

		protected override void OnError(IClientQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(SubscriptionEventJob), ex.Source, ex.Message);

			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt },
				{"resourceGroup", item.ResourceGroup }
			};

			Instance.Connection.Post(url, d);
		}
	}
}
