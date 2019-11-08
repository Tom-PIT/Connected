using Microsoft.AspNetCore.SignalR;

namespace TomPIT.Cdn.Data
{
	internal static class DataHubs
	{
		internal static IHubContext<DataHub> Data { get; set; }
	}
}
