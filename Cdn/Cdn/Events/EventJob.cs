using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace TomPIT.Cdn.Events
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

			var ctx = new MicroServiceContext(ms, MiddlewareDescriptor.Current.Tenant.Url);
			var responses = new List<IOperationResponse>();
			IDistributedEventMiddleware eventInstance = null;

			if (string.Compare(ed.Name, "$", true) != 0)
			{
				var eventName = $"{ms.Name}/{ed.Name}";
				var targets = EventHandlers.Query(eventName);

				eventInstance = CreateEventInstance(ctx, ed);

				if (eventInstance != null)
				{
					if (!eventInstance.Invoking())
						return;

					eventInstance.Invoke();
				}

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

			Notify(ms, ed, responses);
		}

		private void Notify(IMicroService microService, EventDescriptor descriptor, List<IOperationResponse> responses)
		{
			if (responses.Count > 0)
			{
				foreach (var response in responses)
				{
					if (response.Result == ResponseResult.Objection)
						return;
				}
			}

			Task.Run(async () =>
			{
				await MiddlewareDescriptor.Current.Tenant.GetService<IEventHubService>().NotifyAsync(new EventHubNotificationArgs($"{microService.Name}/{descriptor.Name}", descriptor.Arguments));
			});
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
				ctx.Services.Diagnostic.Error(ed.Name, ex.Message, LogCategories.Cdn);
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

			var context = new MicroServiceContext(i.Configuration().MicroService(), MiddlewareDescriptor.Current.Tenant.Url);
			var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(i.Configuration().MicroService(), i, i.Name, false);

			if (type == null)
			{
				context.Services.Diagnostic.Warning(ed.Name, $"{SR.ErrTypeExpected} ({i.Name})", nameof(Invoke));
				return null;
			}

			try
			{
				var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IEventMiddleware>(context, type, ed.Arguments);

				handler.Invoke(ed.Name);

				return handler.Responses;
			}
			catch (Exception ex)
			{
				context.Services.Diagnostic.Error($"{i.Event}:{i.Name}", ex.Message, LogCategories.Cdn);
			}

			return null;
		}

		protected override void OnError(IQueueMessage item, Exception ex)
		{
			if (ex is ValidationException val)
			{
				var urlComplete = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Complete");
				var descriptorComplete = new JObject
				{
					{"popReceipt", item.PopReceipt }
				};

				MiddlewareDescriptor.Current.Tenant.Post(urlComplete, descriptorComplete);
				return;
			}

			MiddlewareDescriptor.Current.Tenant.LogError(ex.Source, ex.Message, LogCategories.Cdn);

			var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("EventManagement", "Ping");
			var d = new JObject
			{
				{"popReceipt", item.PopReceipt }
			};

			MiddlewareDescriptor.Current.Tenant.Post(url, d);
		}

		private IDistributedEventMiddleware CreateEventInstance(IMicroServiceContext context, EventDescriptor eventDescriptor)
		{
			var compiler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>();
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
