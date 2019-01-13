using Amt.DataHub.Authentication;
using Amt.DataHub.Transactions;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Amt.DataHub.Hubs
{
	[WorkersAuthentication]
	public class WorkersHub : Hub
	{
		public override Task OnConnected()
		{
			var hid = Context.Headers["WorkerId"].AsGuid();

			DataHubModel.RegisterWorkerInstance(hid, Context.ConnectionId);

			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled)
		{
			DataHubModel.RemoveWorkerInstance(Context.ConnectionId);

			return base.OnDisconnected(stopCalled);
		}

		public void Renew()
		{
			DataHubModel.RenewWorkerInstance(Context.ConnectionId);
		}

		public void StartTaskConfirm(TaskArgs e)
		{

		}
	}
}