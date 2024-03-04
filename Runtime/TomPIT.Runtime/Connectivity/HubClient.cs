using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace TomPIT.Connectivity
{
	public abstract class HubClient : ClientConnection
	{
		public HubClient(ITenant tenant, string authenticationToken) : base(tenant)
		{
			AuthenticationToken = authenticationToken;
		}
		protected string AuthenticationToken { get; }

		protected override Action<HttpConnectionOptions> Options => (f) =>
		{
			f.AccessTokenProvider = () =>
			{
				return Task.FromResult(AuthenticationToken);
			};

			f.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;

			f.Headers.Add("TomPITInstanceId", Instance.Id.ToString());
		};

		protected override string Url => string.Format("{0}/{1}", Tenant.Url, HubName);

		private void Heartbeat()
		{
			while (!Cancel.IsCancellationRequested)
			{
				try
				{
					if (Hub == null)
						return;

					Hub.InvokeAsync("Heartbeat").Wait();
				}
				finally
				{
					Cancel.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30));
				}
			}
		}

		protected override void OnConnected()
		{
			new Task(Heartbeat, Cancel.Token, TaskCreationOptions.LongRunning).Start();
		}
	}
}
