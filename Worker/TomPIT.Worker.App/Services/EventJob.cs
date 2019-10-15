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
		public EventJob(Dispatcher<IQueueMessage> owner, CancellationTokenSource cancel) : base(owner, cancel)
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

			var eventInstance = CreateEventInstance(ed);

			if (eventInstance != null)
				eventInstance.Invoke();

			var responses = new List<IOperationResponse>();

			if (string.Compare(ed.Name, "$", true) != 0)
			{
				var targets = EventHandlers.Query(ed.Name);

				if (targets != null)
				{
					foreach (var target in targets)
					{
						if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(target.Item2) is IEventBindingConfiguration configuration))
							return;

						Parallel.ForEach(configuration.Events,
							(i) =>
							{
								if (ed.Name.Equals(i.Event, StringComparison.OrdinalIgnoreCase))
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
			var op = descriptor.Configuration.Operations.FirstOrDefault(f => f.Id == new Guid(descriptor.Element));

			if (op == null)
				return;

			var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(op, ed.Arguments);
			var property = instance.GetType().GetProperty(nameof(IDistributedOperation.OperationTarget));

			if (property.SetMethod == null)
				return;

			property.SetMethod.Invoke(instance, new object[] { DistributedOperationTarget.InProcess });

			if (responses != null && responses.Count > 0)
				instance.Responses.AddRange(responses);

			instance.Invoke();
		}

		private List<IOperationResponse> Invoke(EventDescriptor ed, IEventBinding i)
		{
			var ctx = new MicroServiceContext(i.Configuration().MicroService());
			var configuration = i.Closest<IEventBindingConfiguration>();

			if (configuration == null)
			{
				ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrElementClosestNull} ({nameof(IEventBinding)}->{nameof(IEventBindingConfiguration)}");
				return null;
			}

			var componentName = configuration.ComponentName();
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(ctx.MicroService.Token, configuration, componentName, false);

			if (type == null)
			{
				ctx.Services.Diagnostic.Warning(nameof(EventJob), nameof(Invoke), $"{SR.ErrTypeExpected} ({componentName})");
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
				ctx.Services.Diagnostic.Error(nameof(EventJob), nameof(Invoke), $"{ex.Message}");
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
			MiddlewareDescriptor.Current.Tenant.LogError(nameof(EventJob), ex.Source, ex.Message);

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
			var context = MicroServiceContext.FromIdentifier(eventDescriptor.Name, MiddlewareDescriptor.Current.Tenant);
			var descriptor = ComponentDescriptor.DistributedEvent(context, eventDescriptor.Name);

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
