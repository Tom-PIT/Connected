using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
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

			Instance.Tenant.GetService<ISubscriptionWorkerService>().CompleteEvent(item.PopReceipt);
		}

		private void Invoke(JObject message)
		{
			var subscriptionEvent = Instance.Tenant.GetService<ISubscriptionWorkerService>().SelectEvent(message.Required<Guid>("id"));

			if (subscriptionEvent == null)
				return;

			if (!(Instance.Tenant.GetService<IComponentService>().SelectConfiguration(subscriptionEvent.Handler) is ISubscriptionConfiguration config))
				return;

			var eventConfig = config.Events.FirstOrDefault(f => string.Compare(f.Name, subscriptionEvent.Name, true) == 0);

			if (eventConfig == null)
				return;

			var ctx = MiddlewareDescriptor.Current.CreateContext(config.MicroService());

			var middleware = Instance.Tenant.GetService<ICompilerService>().CreateInstance<ISubscriptionEventMiddleware>(ctx, eventConfig, message.Optional("arguments", string.Empty));

			middleware.Event = subscriptionEvent;
			middleware.Recipients = Instance.Tenant.GetService<ISubscriptionWorkerService>().QueryRecipients(subscriptionEvent.Handler, subscriptionEvent.PrimaryKey, subscriptionEvent.Topic);

			middleware.Invoke();
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Tenant.LogError(nameof(SubscriptionEventJob), ex.Source, ex.Message);
			Instance.Tenant.GetService<ISubscriptionWorkerService>().PingSubscriptionEvent(item.PopReceipt);
		}
	}
}
