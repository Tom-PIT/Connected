using Microsoft.AspNetCore.SignalR;

namespace TomPIT.IoT.Hubs
{
	internal static class IoTHubs
	{
		internal static IHubContext<IoTServerHub> IoT { get; set; }

	}
}
