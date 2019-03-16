using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class SubscriptionJob : DispatcherJob<IClientQueueMessage>
	{
		public SubscriptionJob(Dispatcher<IClientQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IClientQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = Instance.Connection.CreateUrl("SubscriptionManagement", "Complete");
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
			var u = Instance.Connection.CreateUrl("SubscriptionManagement", "Select");
			var e = new JObject
			{
				{"token",  id}
			};

			var sub = Instance.Connection.Post<SubscriptionDescriptor>(u, e);

			if (sub == null)
				return;

			var config = Instance.Connection.GetService<IComponentService>().SelectConfiguration(sub.Handler) as ISubscription;
			var ms = config.MicroService(Instance.Connection);
			var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, Instance.GetService<IMicroServiceService>().Select(ms), string.Empty);
			var subscribeArgs = new SubscriptionSubscribeArguments(ctx, sub);
			var subscribedArgs = new SubscriptionSubscribedArguments(ctx, sub);

			Instance.GetService<ICompilerService>().Execute(ms, config.Subscribe, this, subscribeArgs);

			if (subscribeArgs.Subscribers.Count > 0)
			{
				u = Instance.Connection.CreateUrl("Subscription", "InsertSubscribers");
				e = new JObject
				{
					{"subscription", sub.Token }
				};

				var a = new JArray();

				e.Add("items", a);

				foreach (var recipient in subscribeArgs.Subscribers)
				{
					a.Add(new JObject
					{
						{"type", recipient.Type.ToString() },
						{"resourcePrimaryKey", recipient.ResourcePrimaryKey }
					});
				}

				Instance.Connection.Post(u, e);
			}

			Instance.GetService<ICompilerService>().Execute(ms, config.Subscribed, this, subscribedArgs);
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
