using Microsoft.AspNetCore.SignalR;

namespace TomPIT.Cdn.Clients
{
	internal static class ClientHubs
	{
		internal static IHubContext<ClientHub> Clients { get; set; }
	}
}
