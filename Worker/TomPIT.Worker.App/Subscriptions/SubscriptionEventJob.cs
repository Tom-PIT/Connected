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
		public SubscriptionEventJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(m);

			MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().CompleteEvent(item.PopReceipt);
		}

		private void Invoke(JObject message)
		{
			var subscriptionEvent = MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().SelectEvent(message.Required<Guid>("id"));

			if (subscriptionEvent == null)
				return;

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(subscriptionEvent.Handler) is ISubscriptionConfiguration config))
				return;

			var eventConfig = config.Events.FirstOrDefault(f => string.Compare(f.Name, subscriptionEvent.Name, true) == 0);

			if (eventConfig == null)
				return;
			
			using var ctx = new MicroServiceContext(config.MicroService());
			var middleware = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<ISubscriptionEventMiddleware>(ctx, eventConfig, subscriptionEvent.Arguments, eventConfig.Name);

			middleware.Event = subscriptionEvent;
			middleware.Recipients = MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().QueryRecipients(subscriptionEvent.Handler, subscriptionEvent.PrimaryKey, subscriptionEvent.Topic);

			middleware.Invoke();
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(SubscriptionEventJob));
			MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().PingSubscriptionEvent(item.PopReceipt);
		}
	}
}
