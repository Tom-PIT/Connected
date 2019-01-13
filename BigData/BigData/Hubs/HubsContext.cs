using Microsoft.AspNet.SignalR;
using System;

namespace Amt.DataHub.Hubs
{
	internal static class HubsContext
	{
		private static readonly Lazy<IHubContext> _workers = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<WorkersHub>());

		public static IHubContext WorkersHub { get { return _workers.Value; } }
	}
}