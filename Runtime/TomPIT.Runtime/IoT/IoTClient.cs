using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Connectivity;
using TomPIT.Notifications;

namespace TomPIT.IoT
{
	internal class IoTClient : HubClient
	{
		public IoTClient(ISysConnection connection, string authenticationToken) : base(connection, authenticationToken)
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

				if (Connection.GetService<IIoTService>() is IIoTServiceNotification n)
					n.NotifyStateChanged(Connection, e.Args);
			});
		}
	}
}
