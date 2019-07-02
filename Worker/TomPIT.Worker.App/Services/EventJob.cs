using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Events;
using TomPIT.Services;
using TomPIT.Services.Context;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class EventJob : DispatcherJob<IQueueMessage>
	{
		public EventJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = Instance.Connection.CreateUrl("EventManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var id = data.Required<Guid>("id");
			var url = Instance.Connection.CreateUrl("EventManagement", "Select")
				.AddParameter("id", id);

			var ed = Instance.Connection.Get<EventDescriptor>(url);

			if (ed == null)
				return;

			if (string.Compare(ed.Name, "$", true) != 0)
			{
				var targets = EventHandlers.Query(ed.Name);

				if (targets != null)
				{
					Parallel.ForEach(targets,
						(f) =>
						{
							if (!(Instance.GetService<IComponentService>().SelectConfiguration(f.Item2) is IEventHandler configuration))
								return;

							foreach (var i in configuration.Events)
							{
								if (ed.Name.Equals(i.Event, StringComparison.OrdinalIgnoreCase))
									Invoke(ed, i);
							}
						});
				}
			}

			if (ed.Callback != null)
				Callback(ed);
		}

		private void Callback(EventDescriptor ed)
		{
			var tokens = ed.Callback.Split('/');

			if (!(Instance.GetService<IComponentService>().SelectConfiguration(tokens[1].AsGuid()) is IApi api))
				return;

			var op = api.Operations.FirstOrDefault(f => f.Id == tokens[2].AsGuid());

			if (op == null)
				return;

			var args = string.IsNullOrWhiteSpace(ed.Arguments)
				? null
				: JsonConvert.DeserializeObject<JObject>(ed.Arguments);

			var microService = Instance.GetService<IMicroServiceService>().Select(tokens[0].AsGuid());
			var ctx = TomPIT.Services.ExecutionContext.Create(Instance.Connection.Url, microService);
			var ai = new ApiInvoke(ctx);

			ai.Execute(null, microService.Token, api.ComponentName(Instance.Connection), op.Name, args, null, true, true);
		}

		private void Invoke(EventDescriptor ed, IEventBinding i)
		{
			var ctx = TomPIT.Services.ExecutionContext.Create(Instance.Connection.Url, null);

			var args = string.IsNullOrWhiteSpace(ed.Arguments)
				? null
				: JsonConvert.DeserializeObject<JObject>(ed.Arguments);

			var e = new EventInvokeArguments(ctx, ed.Name, args);

			Instance.GetService<ICompilerService>().Execute(i.MicroService(Instance.Connection), i.Closest<IEventHandler>().Invoke, this, e);
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Connection.LogError(nameof(EventJob), ex.Source, ex.Message);

			var url = Instance.Connection.CreateUrl("EventManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Connection.Post(url, d);
		}
	}
}
