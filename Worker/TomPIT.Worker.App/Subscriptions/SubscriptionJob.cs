using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Cdn;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Subscriptions
{
	internal class SubscriptionJob : DispatcherJob<IQueueMessage>
	{
		public SubscriptionJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(m);

			Instance.Connection.GetService<ISubscriptionWorkerService>().CompleteSubscription(item.PopReceipt);
		}

		private void Invoke(JObject message)
		{
			var sub = Instance.Connection.GetService<ISubscriptionWorkerService>().SelectSubscription(message.Required<Guid>("id"));

			if (sub == null)
				return;

			var config = Instance.Connection.GetService<IComponentService>().SelectConfiguration(sub.Handler) as ComponentModel.Cdn.ISubscription;
			var ms = config.MicroService(Instance.Connection);
			var type = Instance.GetService<ICompilerService>().ResolveType(ms, config, config.ComponentName(Instance.Connection));
			var handler = Instance.Connection.CreateProcessHandler<ISubscriptionHandler>(ms, type);

			Instance.Connection.GetService<ISubscriptionWorkerService>().InsertSubscribers(sub.Token, handler.Invoke(sub));

			handler.Created(sub);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(SubscriptionEventJob), ex.Source, ex.Message);
			Instance.Connection.GetService<ISubscriptionWorkerService>().PingSubscription(item.PopReceipt);
		}
	}
}
