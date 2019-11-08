using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Connectivity;
using TomPIT.Messaging;

namespace TomPIT.IoT
{
	internal class IoTClient : HubClient
	{
		public IoTClient(ITenant tenant, string authenticationToken) : base(tenant, authenticationToken)
		{
		}

		protected override string HubName => "iot";

		protected override void Initialize()
		{
			IoTState();
		}

		private void IoTState()
		{
			Hub.On<MessageEventArgs<IoTStateChangedArgs>>("IoTStateChanged", (e) =>
			{
				Hub.InvokeAsync("Confirm", e.Message);

				if (Tenant.GetService<IIoTService>() is IIoTServiceNotification n)
					n.NotifyStateChanged(Tenant, e.Args);
			});
		}
	}
}
