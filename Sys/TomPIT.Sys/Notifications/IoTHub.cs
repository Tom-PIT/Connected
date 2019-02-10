using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Notifications
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class IoTHub : Hub
	{
		public IoTHub(IHubContext<IoTHub> context)
		{
			IoTNotifications.Cache = context;
		}

		public override Task OnConnectedAsync()
		{
			DataModel.MessageSubscribers.Insert("iot", Context.ConnectionId, SysExtensions.RequestInstanceId);

			return Task.CompletedTask;
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			DataModel.MessageSubscribers.Delete("iot", Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}

		public void Heartbeat()
		{
			DataModel.MessageSubscribers.Heartbeat("iot", Context.ConnectionId);
		}

		public void Confirm(Guid message)
		{
			DataModel.MessageRecipients.Delete(message, Context.ConnectionId);
		}
	}
}
