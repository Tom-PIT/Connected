using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.IoT;
using TomPIT.Notifications;

namespace TomPIT.Connectivity
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

				if (Connection.GetService<IIoTService>() is IIoTNotification n)
					n.NotifyChanged(Connection, e.Args);
			});
		}
	}
}
