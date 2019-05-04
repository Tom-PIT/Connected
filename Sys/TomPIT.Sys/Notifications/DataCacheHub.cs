using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class DataCacheHub : Hub
	{
		public DataCacheHub(IHubContext<CacheHub> context)
		{
			CachingNotifications.Cache = context;
		}

		public override Task OnConnectedAsync()
		{
			DataModel.MessageSubscribers.Insert("datacache", Context.ConnectionId, SysExtensions.RequestInstanceId);

			return Task.CompletedTask;
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			DataModel.MessageSubscribers.Delete("datacache", Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}

		public void Heartbeat()
		{
			DataModel.MessageSubscribers.Heartbeat("datacache", Context.ConnectionId);
		}

		public void Confirm(Guid message)
		{
			DataModel.MessageRecipients.Delete(message, Context.ConnectionId);
		}
	}
}
