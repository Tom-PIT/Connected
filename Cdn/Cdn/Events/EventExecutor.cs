using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Messaging;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.Cdn.Events;

internal sealed class EventExecutor
	: IDisposable
{
	public EventExecutor(IDispatcher<IEventQueueMessage> owner, IEventQueueMessage message)
	{
		Message = message;
		Owner = owner;
		Context = new MicroServiceContext(Message.MicroService);
	}

	private IDispatcher<IEventQueueMessage> Owner { get; }
	private IEventQueueMessage Message { get; }
	private IMicroServiceContext Context { get; }
	private List<IOperationResponse> Responses { get; } = new();
	private IDistributedEventMiddleware Middleware { get; set; } = default!;

	private string EventName => $"{Context.MicroService.Name}/{Message.Name}";
	public bool Invoke()
	{
		if (!string.Equals(Message.Name, "$", StringComparison.OrdinalIgnoreCase))
		{
			if (!TryInvokeMiddleware())
			{
				Middleware.Invoked();
				return false;
			}
		}

		if (!string.IsNullOrWhiteSpace(Message.Callback))
			Callback();

		Middleware.Invoked();

		Notify();

		return true;
	}

	private bool TryInvokeMiddleware()
	{
		var instance = CreateEventInstance();

		if (instance is not null)
			Middleware = instance;
		else
			Middleware = new VirtualEventMiddleware(Context);

		if (Middleware is not null)
		{
			if (Owner.Behavior == ProcessBehavior.Parallel)
			{
				var att = Middleware.GetType().FindAttribute<ProcessBehaviorAttribute>();

				if (att?.Behavior == ProcessBehavior.Queued)
				{
					Owner.Enqueue(att.QueueName, Message);

					return false;
				}
			}

			var args = new DistributedEventInvokingArgs();

			Middleware.Invoking(args);

			switch (args.Result)
			{
				case EventInvokingResult.Cancel:
					return true;
				case EventInvokingResult.Delay:
					Instance.SysProxy.Management.Events.Ping(Message.PopReceipt, args.Delay == TimeSpan.Zero ? 60 : Convert.ToInt32(args.Delay.TotalSeconds));
					return false;
			}

			Middleware.Invoke();
		}

		InvokeHandlers();

		return true;
	}

	private void InvokeHandlers()
	{
		var targets = EventHandlers.Query(EventName);

		if (targets is null)
			return;

		foreach (var target in targets)
		{
			if (MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(target.Item2) is not IEventBindingConfiguration configuration)
				continue;

			foreach (var listener in configuration.Events)
			{
				if (string.Equals(EventName, listener.Event, StringComparison.OrdinalIgnoreCase))
				{
					var result = InvokeBinding(listener);

					if (result is not null && result.Count > 0)
						Responses.AddRange(result);
				}
			}
		}
	}

	private void Notify()
	{
		if (Responses.Count > 0)
		{
			foreach (var response in Responses)
			{
				if (response.Result == ResponseResult.Objection)
					return;
			}
		}

		MiddlewareDescriptor.Current.Tenant.GetService<IEventHubService>().NotifyAsync(new EventHubNotificationArgs($"{Context.MicroService.Name}/{Message.Name}", Message.Arguments));
	}

	private void Callback()
	{
		var isImpersonated = false;

		if (!string.IsNullOrWhiteSpace(Message.Arguments))
		{
			var argumentsState = JObject.Parse(Message.Arguments);

			if (argumentsState.TryGetValue("user$", out var userToken))
			{
				var userId = userToken.Value<string>();

				if (!string.IsNullOrWhiteSpace(userId))
				{
					Context.Impersonate(userId);
					isImpersonated = true;
				}
			}
		}

		var descriptor = ComponentDescriptor.Api(Context, Message.Callback);

		try
		{
			descriptor.Validate();
		}
		catch (RuntimeException ex)
		{
			TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);
		}

		var op = descriptor.Configuration.Operations.FirstOrDefault(f => f.Id == new Guid(descriptor.Element));

		if (op == null)
			return;

		var instance = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IDistributedOperation>(Context, op, Message.Arguments, op.Name);

		ReflectionExtensions.SetPropertyValue(instance, nameof(IDistributedOperation.OperationTarget), DistributedOperationTarget.InProcess);

		if (Responses is not null && Responses.Count > 0)
			instance.Responses.AddRange(Responses);

		instance.Invoke();

		if (isImpersonated)
			Context.RevokeImpersonation();
	}

	private List<IOperationResponse>? InvokeBinding(IEventBinding i)
	{
		if (string.IsNullOrEmpty(i.Name))
			return null;

		var type = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().ResolveType(i.Configuration().MicroService(), i, i.Name, false);

		if (type is null)
		{
			Context.Services.Diagnostic.Warning(Message.Name, $"{SR.ErrTypeExpected} ({i.Name})", nameof(Invoke));
			return null;
		}

		try
		{
			var handler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CreateInstance<IEventMiddleware>(Context, type, Message.Arguments);

			handler.Invoke(Message.Name);

			return handler.Responses;
		}
		catch (Exception ex)
		{
			TomPITException.Unwrap(this, ex).LogError(LogCategories.Cdn);
		}

		return null;
	}


	private IDistributedEventMiddleware? CreateEventInstance()
	{
		var compiler = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>();
		var descriptor = ComponentDescriptor.DistributedEvent(Context, EventName);

		descriptor.Validate();

		if (descriptor.Configuration is null)
			return null;

		if (descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0) is not IDistributedEvent ev)
			throw new RuntimeException($"{SR.ErrDistributedEventNotFound} ({Message.Name})");

		if (compiler.ResolveType(Context.MicroService.Token, ev, ev.Name, false) is not Type type)
			return null;

		return compiler.CreateInstance<IDistributedEventMiddleware>(Context, type, Message.Arguments);
	}

	public void Dispose()
	{
		Middleware?.Dispose();
		Context.Dispose();
	}
}
