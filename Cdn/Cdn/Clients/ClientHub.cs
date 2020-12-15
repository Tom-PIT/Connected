using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Clients
{
	public class ClientHub : Hub
	{
		public ClientHub(IHubContext<ClientHub> context)
		{
			ClientHubs.Clients = context;
		}

		public async Task Add(ClientSubscription e)
		{
			try
			{
				AuthorizeClient(e.Token);

				await Groups.AddToGroupAsync(Context.ConnectionId, e.Token.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Remove(ClientSubscription e)
		{
			try
			{
				await Groups.RemoveFromGroupAsync(Context.ConnectionId, e.Token.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		private void AuthorizeClient(string token)
		{
			var descriptor = MiddlewareDescriptor.Current.Tenant.GetService<IClientService>().Select(token);

			if (descriptor == null)
				throw new NotFoundException($"{SR.ErrClientNotFound} ({token})");
		}
	}
}
