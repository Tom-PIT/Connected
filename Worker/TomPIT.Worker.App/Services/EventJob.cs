using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Diagnostics;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Messaging;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class EventJob : DispatcherJob<IQueueMessage>
	{
		public EventJob(Dispatcher<IQueueMessage> owner, CancellationToken cancel) : base(owner, cancel)
		{
		}

		protected override void DoWork(IQueueMessage item)
		{
			var m = JsonConvert.DeserializeObject(item.Message) as JObject;

			Invoke(item, m);

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Complete");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		private void Invoke(IQueueMessage queue, JObject data)
		{
			var id = data.Required<Guid>("id");
			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Select")
				.AddParameter("id", id);

			var ed = MiddlewareDescriptor.Current.Tenant.Get<EventDescriptor>(url);

			if (ed == null)
				return;

			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(ed.MicroService);

			if (ms == null)
				return;

			var responses = new List<IOperationResponse>();
			IDistributedEventMiddleware eventInstance = null;

			if (string.Compare(ed.Name, "$", true) != 0)
			{
				var eventName = $"{ms.Name}/{ed.Name}";
				eventInstance = CreateEventInstance(ed);

				if (eventInstance != null)
				{
					if (!eventInstance.Invoking())
						return;

					eventInstance.Invoke();
				}

				var targets = EventHandlers.Query(eventName);

				if (targets != null)
				{
					foreach (var target in targets)
					{
						if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(target.Item2) is IEventBindingConfiguration configuration))
							return;

						Parallel.ForEach(configuration.Events,
							(i) =>
							{
								if (string.Compare(eventName, i.Event, true) == 0)
								{
									var result = Invoke(ed, i);

									if (result != null && result.Count > 0)
									{
										lock (responses)
										{
											responses.AddRange(result);
										}
									}
								}
							});
					};
				}
			}

			if (!string.IsNullOrWhiteSpace(ed.Callback))
				Callback(ed, responses);

			if (eventInstance != null)
				eventInstance.Invoked();
		}

		private void Callback(EventDescriptor ed, List<IOperationResponse> responses)
		{
			var ctx = new MicroServiceContext(new Guid(ed.Callback.Split('/')[0]));
			var descriptor = ComponentDescriptor.Api(ctx, ed.Callback);

			try
			{
				descriptor.Validate();
			}
			catch (RuntimeException ex)
			{
				ctx.Services.Diagnostic.Error(nameof(EventJob), ex.Message, LogCategories.Worker);
			}

			var op = descriptor.Configuration.Operations.FirstOrDefault(f => f.Id == new Guid(descriptor.Element));

			if (op == null)
				return;

			var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(ctx, op, ed.Arguments, op.Name);

			ReflectionExtensions.SetPropertyValue(instance, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

			if (responses != null && responses.Count > 0)
				instance.Responses.AddRange(responses);

			instance.Invoke();
		}

		private List<IOperationResponse> Invoke(EventDescriptor ed, IEventBinding i)
		{
			if (string.IsNullOrEmpty(i.Name))
				return null;

			var ctx = new MicroServiceContext(i.Configuration().MicroService());
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ctx.MicroService.Token, i, i.Name, false);

			if (type == null)
			{
				ctx.Services.Diagnostic.Warning(nameof(EventJob), $"{SR.ErrTypeExpected} ({i.Name})", nameof(Invoke));
				return null;
			}

			try
			{
				var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IEventMiddleware>(ctx, type, ed.Arguments);

				Invoke(handler, ed.Name);

				return handler.Responses;
			}
			catch (Exception ex)
			{
				ctx.Services.Diagnostic.Error(nameof(EventJob), ex.Message, nameof(Invoke));
			}

			return null;
		}

		private void Invoke(IEventMiddleware handler, string eventName)
		{
			Exception lastError = null;

			for (var i = 0; i < 10; i++)
			{
				try
				{
					handler.Invoke(eventName);
					break;
				}
				catch (Exception ex)
				{
					lastError = ex;
					Thread.Sleep((i + 1) * 250);
				}
			}

			if (lastError != null)
				throw lastError;
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, nameof(EventJob));

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		private IDistributedEventMiddleware CreateEventInstance(EventDescriptor eventDescriptor)
		{
			var compiler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>();
			var context = new MicroServiceContext(eventDescriptor.MicroService, MiddlewareDescriptor.Current.Tenant.Url);
			var descriptor = ComponentDescriptor.DistributedEvent(context, $"{context.MicroService.Name}/{eventDescriptor.Name}");

			descriptor.Validate();

			var ev = descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (ev == null)
				throw new RuntimeException($"{SR.ErrDistributedEventNotFound} ({eventDescriptor.Name})");

			var type = compiler.ResolveType(context.MicroService.Token, ev, ev.Name, false);

			if (type == null)
				return null;

			return compiler.CreateInstance<IDistributedEventMiddleware>(context, type, eventDescriptor.Arguments);
		}
	}
}
