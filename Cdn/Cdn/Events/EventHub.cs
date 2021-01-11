using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Distributed;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Events
{
	//[Authorize(AuthenticationSchemes = "TomPIT")]
	public class EventHub : Hub
	{
		public EventHub(IHubContext<EventHub> context)
		{
			EventHubs.Events = context;
		}

		public async Task Add(List<EventHubSubscription> events)
		{
			try
			{
				foreach (var e in events)
					AuthorizeEvent(e.Name);

				foreach (var e in events)
					await Groups.AddToGroupAsync(Context.ConnectionId, e.Name.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Remove(List<EventHubSubscription> events)
		{
			try
			{
				foreach (var e in events)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, e.Name.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		private void AuthorizeEvent(string eventName)
		{
			using var ctx = new MiddlewareContext();
			var descriptor = ComponentDescriptor.DistributedEvent(ctx, eventName);

			descriptor.Validate();

			if (descriptor.Configuration == null)
				throw new NotFoundException($"{SR.ErrCannotFindConfiguration} ({eventName})");

			var target = descriptor.Configuration.Events.FirstOrDefault(f => string.Compare(f.Name, descriptor.Element, true) == 0);

			if (target == null)
				throw new NotFoundException($"{SR.ErrDistributedEventNotFound} ({eventName})");

			var type = descriptor.Context.Tenant.GetService<ICompilerService>().ResolveType(descriptor.MicroService.Token, target, target.Name, false);

			if (type != null)
			{
				var instance = descriptor.Context.CreateMiddleware<IDistributedEventMiddleware>(type);

				instance.Authorize(new EventConnectionArgs(Context.ConnectionId));
			}
		}
	}
}
