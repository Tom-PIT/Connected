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

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionEventJob : DispatcherJob<IQueueMessage>
	{
		public SubscriptionEventJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(m);

			Instance.GetService<ISubscriptionWorkerService>().CompleteEvent(item.PopReceipt);
		}

		private void Invoke(JObject message)
		{
			var subscriptionEvent = Instance.GetService<ISubscriptionWorkerService>().SelectEvent(message.Required<Guid>("id"));

			if (subscriptionEvent == null)
				return;

			if (!(Instance.Connection.GetService<IComponentService>().SelectConfiguration(subscriptionEvent.Handler) is TomPIT.ComponentModel.Cdn.ISubscription config))
				return;

			var eventConfig = config.Events.FirstOrDefault(f => string.Compare(f.Name, subscriptionEvent.Name, true) == 0);

			if (eventConfig == null)
				return;

			var instance = Instance.Connection.CreateProcessHandler<ISubscriptionEventHandler>(eventConfig, subscriptionEvent.Name, message.Optional("arguments", string.Empty));

			instance.Event = subscriptionEvent;
			instance.Recipients = Instance.GetService<ISubscriptionWorkerService>().QueryRecipients(subscriptionEvent.Handler, subscriptionEvent.PrimaryKey, subscriptionEvent.Topic);

			instance.Invoke();
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(SubscriptionEventJob), ex.Source, ex.Message);
			Instance.Connection.GetService<ISubscriptionWorkerService>().PingSubscriptionEvent(item.PopReceipt);
		}
	}
}
