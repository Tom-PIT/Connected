using Microsoft.AspNetCore.SignalR.Client;
using TomPIT.Connectivity;
using TomPIT.Notifications;

namespace TomPIT.IoT.Connectivity
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

				//if (Connection.GetService<IAuthorizationService>() is IAuthenticationTokenNotification n)
				//	n.NotifyAuthenticationTokenChanged(Connection, e.Args);
			});
		}
	}
}
