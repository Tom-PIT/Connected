using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Printing
{
	[Authorize(AuthenticationSchemes = "TomPIT")]
	public class PrintingHub : Hub
	{
		public PrintingHub(IHubContext<PrintingHub> context)
		{
			PrintingHubs.Printing = context;
		}

		public async Task Add(List<PrintingHubSubscription> printers)
		{
			try
			{
				foreach (var printer in printers)
					await Groups.AddToGroupAsync(Context.ConnectionId, printer.Name.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Remove(List<PrintingHubSubscription> printers)
		{
			try
			{
				foreach (var printer in printers)
					await Groups.RemoveFromGroupAsync(Context.ConnectionId, printer.Name.ToLowerInvariant());

				await Task.CompletedTask;
			}
			catch (Exception ex)
			{
				await Clients.Caller.SendAsync("exception", ex.Message);
			}
		}

		public async Task Complete(Guid popReceipt)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Complete(popReceipt);

			await Task.CompletedTask;
		}

		public async Task Ping(Guid popReceipt)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<IPrintingSpoolerManagementService>().Ping(popReceipt);

			await Task.CompletedTask;
		}
	}
}
