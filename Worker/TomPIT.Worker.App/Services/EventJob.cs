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
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class EventJob : DispatcherJob<IClientQueueMessage>
	{
		public EventJob(Dispatcher<IClientQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IClientQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = Instance.Connection.CreateUrl("EventManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt },
				{"resourceGroup", item.ResourceGroup }
			};

			Instance.Connection.Post(url, d);
		}

		private void Invoke(IClientQueueMessage queue, JObject data)
		{
			var id = data.Required<Guid>("id");
			var url = Instance.Connection.CreateUrl("EventManagement", "Select")
				.AddParameter("id", id);

			var ed = Instance.Connection.Get<EventDescriptor>(url);

			if (ed == null)
				return;

			var targets = EventHandlers.Query(ed.Name);

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

			var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, Instance.GetService<IMicroServiceService>().Select(tokens[0]), string.Empty);
			var opArgs = new OperationInvokeArguments(ctx, op, args, null);

			ctx.Invoke(api.ComponentName(ctx), args);
		}

		private void Invoke(EventDescriptor ed, IEventBinding i)
		{
			try
			{
				var ctx = TomPIT.Services.ExecutionContext.NonHttpContext(Instance.Connection.Url, null, string.Empty);

				var args = string.IsNullOrWhiteSpace(ed.Arguments)
					? null
					: JsonConvert.DeserializeObject<JObject>(ed.Arguments);

				var e = new EventInvokeArguments(ctx, ed.Name, args);

				Instance.GetService<ICompilerService>().Execute(i.MicroService(Instance.Connection), i.Closest<IEventHandler>().Invoke, this, e);
			}
			catch (Exception)
			{
				//Instance.Sys.LogError(ctx, SR.LogCategoryWorker, nameof(Invoke), string.Format("{0} ({1}.{2}.{3})", SR.ErrWorkerNotFound, service, api, operation));
			}
		}
	}
}
