using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class CacheHub : Hub
	{
		public CacheHub(IHubContext<CacheHub> context)
		{
			NotificationHubs.Cache = context;
		}

		public override Task OnConnectedAsync()
		{
			DataModel.MessageSubscribers.Insert("cache", Context.ConnectionId);

			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			DataModel.MessageSubscribers.Delete("cache", Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}

		public void Heartbeat()
		{
			DataModel.MessageSubscribers.Heartbeat("cache", Context.ConnectionId);
		}

		public void Confirm(Guid message)
		{
			DataModel.MessageRecipients.Delete(message, Context.ConnectionId);
		}
	}
}
