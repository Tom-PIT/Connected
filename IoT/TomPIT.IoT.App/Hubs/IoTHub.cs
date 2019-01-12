using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace TomPIT.IoT.Hubs
{
	[Authorize]
	public class IoTHub : Microsoft.AspNetCore.SignalR.Hub
	{
		public IoTHub(IHubContext<IoTHub> context)
		{
			IoTHubs.IoT = context;
		}

		public override Task OnConnectedAsync()
		{
			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			return base.OnDisconnectedAsync(exception);
		}

	}
}
