using System;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT;
using TomPIT.Services;
using TomPIT.Storage;

namespace Search.Services
{
	internal class IndexingJob : DispatcherJob<IQueueMessage>
	{
		public IndexingJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
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
			//var id = data.Required<Guid>("id");
			//var url = Instance.Connection.CreateUrl("EventManagement", "Select")
			//	.AddParameter("id", id);

			//var ed = Instance.Connection.Get<EventDescriptor>(url);

			//if (ed == null)
			//	return;

			//var cancel = false;

			//if (string.Compare(ed.Name, "$", true) != 0)
			//{
			//	var targets = EventHandlers.Query(ed.Name);

			//	if (targets != null)
			//	{
			//		foreach (var target in targets)
			//		{
			//			if (!(Instance.GetService<IComponentService>().SelectConfiguration(target.Item2) is IEventHandler configuration))
			//				return;

			//			Parallel.ForEach(configuration.Events,
			//				(i) =>
			//				{
			//					if (ed.Name.Equals(i.Event, StringComparison.OrdinalIgnoreCase))
			//					{
			//						if (Invoke(ed, i) & !cancel)
			//							cancel = true;
			//					}
			//				});
			//		};
			//	}
			//}
		}

		//private bool Invoke(EventDescriptor ed, IEventBinding i)
		//{
		//	var ms = Instance.Connection.GetService<IMicroServiceService>().Select(i.MicroService(Instance.Connection));
		//	var ctx = TomPIT.Services.ExecutionContext.Create(Instance.Connection.Url, ms);
		//	var configuration = i.Closest<IEventHandler>();

		//	if (configuration == null)
		//	{
		//		ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrElementClosestNull} ({nameof(IEventBinding)}->{nameof(IEventHandler)}");
		//		return false;
		//	}

		//	var componentName = configuration.ComponentName(Instance.Connection);
		//	var type = Instance.GetService<ICompilerService>().ResolveType(ms.Token, configuration, componentName, false);

		//	if (type == null)
		//	{
		//		ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrTypeExpected} ({componentName})");
		//		return false;
		//	}

		//	var dataContext = new DataModelContext(ctx);

		//	try
		//	{
		//		var handler = type.CreateInstance<Notifications.IEventHandler>(new object[] { dataContext, ed.Name });

		//		if (!string.IsNullOrWhiteSpace(ed.Arguments))
		//			Types.Populate(ed.Arguments, handler);

		//		Invoke(handler);

		//		if (handler.Cancel)
		//		{
		//			var args = string.IsNullOrWhiteSpace(ed.Arguments)
		//				? new JObject()
		//				: Types.Deserialize<JObject>(ed.Arguments);

		//			var property = args.Property("cancel", StringComparison.OrdinalIgnoreCase);

		//			if (property == null)
		//				args.Add("cancel", true);
		//			else
		//				property.Value = true;

		//			ed.Arguments = Types.Serialize(args);

		//			return handler.Cancel;
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		ctx.Services.Diagnostic.Error(nameof(EventJob), nameof(Invoke), $"{ex.Message}");
		//	}

		//	return false;
		//}

		//private void Invoke(Notifications.IEventHandler handler)
		//{
		//	Exception lastError = null;

		//	for (var i = 0; i < 10; i++)
		//	{
		//		try
		//		{
		//			handler.Invoke();

		//			return;
		//		}
		//		catch (Exception ex)
		//		{
		//			lastError = ex;
		//			Thread.Sleep((i + 1) * 250);
		//		}
		//	}

		//	throw lastError;
		//}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			//	Instance.Connection.LogError(nameof(EventJob), ex.Source, ex.Message);

			//	var url = Instance.Connection.CreateUrl("EventManagement", "Ping");
			//	var d = new JObject
			//	{
			//		{"popReceipt", item.PopReceipt }
			//	};

			//	Instance.Connection.Post(url, d);
			//}
		}
	}
}