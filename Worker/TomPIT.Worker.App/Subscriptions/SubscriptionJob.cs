using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionJob : DispatcherJob<IQueueMessage>
	{
		public SubscriptionJob(IDispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(m);

			MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().CompleteSubscription(item.PopReceipt);
		}

		private void Invoke(JObject message)
		{
			var sub = MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionService>().SelectSubscription(message.Required<Guid>("id"));

			if (sub == null)
				return;

			var config = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(sub.Handler) as ISubscriptionConfiguration;
			using var ctx = new MicroServiceContext(config.MicroService());
			var middleware = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<ISubscriptionMiddleware>(ctx, config, message.Optional("arguments", string.Empty));

			MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().InsertSubscribers(sub.Token, middleware.Invoke(sub));

			middleware.Created(sub);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(SubscriptionEventJob));
			MiddlewareDescriptor.Current.Tenant.GetService<ISubscriptionWorkerService>().PingSubscription(item.PopReceipt);
		}
	}
}
