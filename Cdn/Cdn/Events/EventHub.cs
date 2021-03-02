using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Middleware;
using TomPIT.Security;

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
				var user = Guid.Empty;

				if (Context.User?.Identity is Identity identity && identity.User != null)
					user = identity.User.Token;

				foreach (var e in events)
					MiddlewareDescriptor.Current.Tenant.GetService<IEventHubService>().Authorize(Context.ConnectionId, e.Name, user, e.Authorization);

				foreach (var e in events)
				{
					EventClients.AddOrUpdate(new EventClient
					{
						ConnectionId = Context.ConnectionId,
						User = user,
						EventName = e.Name,
						Arguments = e.Arguments,
						Client = e.Client,
						Behavior = e.Behavior
					});
				}

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
				{
					EventClients.Remove(Context.ConnectionId, e.Name);
					EventMessagingCache.Remove(e.Client, e.Name);
				}

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Acknowledge(EventAcknowledgeArgs e)
		{
			EventMessagingCache.Remove(e);

			await Task.CompletedTask;
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			EventClients.Remove(Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}
	}
}
