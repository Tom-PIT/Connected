using Microsoft.AspNetCore.SignalR;

namespace TomPIT.Cdn.Printing
{
	public static class PrintingHubs
	{
		internal static IHubContext<PrintingHub> Printing { get; set; }
	}
}
