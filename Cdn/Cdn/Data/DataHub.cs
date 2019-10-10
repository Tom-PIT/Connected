using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Data
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class DataHub : Hub
	{
		public DataHub(IHubContext<DataHub> context)
		{
			DataHubs.Data = context;
		}

		public override Task OnConnectedAsync()
		{
			var qs = Context.GetHttpContext().Request.Query;

			var ms = string.Empty;
			var hub = string.Empty;

			if (qs.ContainsKey("microService"))
				ms = qs["microService"];

			if (qs.ContainsKey("hub"))
				hub = qs["hub"];

			if (!string.IsNullOrWhiteSpace(ms) && !string.IsNullOrWhiteSpace(hub))
				Groups.AddToGroupAsync(Context.ConnectionId, string.Format("{0}/{1}", ms.ToLowerInvariant(), hub.ToLowerInvariant()));

			return Task.CompletedTask;
		}

		public override Task OnDisconnectedAsync(Exception exception)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IDataHubService>().Disconnect(Context.ConnectionId);

			return base.OnDisconnectedAsync(exception);
		}

		public async Task Configure(List<DataHubEndpointSubscriber> endpoints)
		{
			var qs = Context.GetHttpContext().Request.Query;

			var ms = string.Empty;
			var hub = string.Empty;

			if (qs.ContainsKey("microService"))
				ms = qs["microService"];

			if (qs.ContainsKey("hub"))
				hub = qs["hub"];

			MiddlewareDescriptor.Current.Tenant.GetService<IDataHubService>().Connect(ms, hub, Context.ConnectionId, endpoints);

			await Task.CompletedTask;
		}
	}
}
