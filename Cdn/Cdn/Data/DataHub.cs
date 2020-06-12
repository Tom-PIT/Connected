using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TomPIT.Cdn.Data
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class DataHub : Hub
	{
		public DataHub(IHubContext<DataHub> context)
		{
			DataHubs.Data = context;
		}

		public async Task Configure(List<DataHubEvent> events)
		{
			foreach (var e in events)
				await Groups.AddToGroupAsync(Context.ConnectionId, e.Name.ToLowerInvariant());

			await Task.CompletedTask;
		}
	}
}
