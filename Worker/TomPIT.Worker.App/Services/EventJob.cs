using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Messaging;
using TomPIT.Middleware;
using TomPIT.Middleware.Interop;
using TomPIT.Reflection;
using TomPIT.Serialization;
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

			var url = Instance.Tenant.CreateUrl("EventManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Tenant.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var id = data.Required<Guid>("id");
			var url = Instance.Tenant.CreateUrl("EventManagement", "Select")
				.AddParameter("id", id);

			var ed = Instance.Tenant.Get<EventDescriptor>(url);

			if (ed == null)
				return;

			var cancel = false;

			if (string.Compare(ed.Name, "$", true) != 0)
			{
				var targets = EventHandlers.Query(ed.Name);

				if (targets != null)
				{
					foreach (var target in targets)
					{
						if (!(Instance.Tenant.GetService<IComponentService>().SelectConfiguration(target.Item2) is IEventBindingConfiguration configuration))
							return;

						Parallel.ForEach(configuration.Events,
							(i) =>
							{
								if (ed.Name.Equals(i.Event, StringComparison.OrdinalIgnoreCase))
								{
									if (Invoke(ed, i) & !cancel)
										cancel = true;
								}
							});
					};
				}
			}

			if (!string.IsNullOrWhiteSpace(ed.Callback))
				Callback(ed, cancel);
		}

		private void Callback(EventDescriptor ed, bool cancel)
		{
			var ctx = new MicroServiceContext(new Guid(ed.Callback.Split('/')[0]));
			var descriptor = ComponentDescriptor.Api(ctx, ed.Callback);
			var op = descriptor.Configuration.Operations.FirstOrDefault(f => f.Id == new Guid(descriptor.Element));

			if (op == null)
				return;

			var args = string.IsNullOrWhiteSpace(ed.Arguments)
				? new JObject()
				: JsonConvert.DeserializeObject<JObject>(ed.Arguments);

			if (args.Property("cancel", StringComparison.OrdinalIgnoreCase) == null)
				args.Add("cancel", cancel);
			else
				args["cancel"] = cancel;

			new ApiInvoker(ctx).Invoke(null, descriptor, ed.Arguments, true);
		}

		private bool Invoke(EventDescriptor ed, IEventBinding i)
		{
			var ctx = new MicroServiceContext(i.Configuration().MicroService());
			var configuration = i.Closest<IEventBindingConfiguration>();

			if (configuration == null)
			{
				ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrElementClosestNull} ({nameof(IEventBinding)}->{nameof(IEventBindingConfiguration)}");
				return false;
			}

			var componentName = configuration.ComponentName();
			var type = Instance.Tenant.GetService<ICompilerService>().ResolveType(ctx.MicroService.Token, configuration, componentName, false);

			if (type == null)
			{
				ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrTypeExpected} ({componentName})");
				return false;
			}

			try
			{
				var handler = Instance.Tenant.GetService<ICompilerService>().CreateInstance<IEventMiddleware>(ctx, type, ed.Arguments);

				handler.EventName = ed.Name;

				Invoke(handler);

				if (handler.Cancel)
				{
					var args = string.IsNullOrWhiteSpace(ed.Arguments)
						? new JObject()
						: Serializer.Deserialize<JObject>(ed.Arguments);

					var property = args.Property("cancel", StringComparison.OrdinalIgnoreCase);

					if (property == null)
						args.Add("cancel", true);
					else
						property.Value = true;

					ed.Arguments = Serializer.Serialize(args);

					return handler.Cancel;
				}
			}
			catch (Exception ex)
			{
				ctx.Services.Diagnostic.Error(nameof(EventJob), nameof(Invoke), $"{ex.Message}");
			}

			return false;
		}

		private void Invoke(IEventMiddleware handler)
		{
			Exception lastError = null;

			for (var i = 0; i < 10; i++)
			{
				try
				{
					handler.Invoke();

					return;
				}
				catch (Exception ex)
				{
					lastError = ex;
					Thread.Sleep((i + 1) * 250);
				}
			}

			throw lastError;
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			Instance.Tenant.LogError(nameof(EventJob), ex.Source, ex.Message);

			var url = Instance.Tenant.CreateUrl("EventManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			Instance.Tenant.Post(url, d);
		}
	}
}
