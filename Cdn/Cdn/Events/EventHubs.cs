using Microsoft.AspNetCore.SignalR;

namespace TomPIT.Cdn.Events
{
	internal static class EventHubs
	{
		internal static IHubContext<EventHub> Events { get; set; }
	}
}
