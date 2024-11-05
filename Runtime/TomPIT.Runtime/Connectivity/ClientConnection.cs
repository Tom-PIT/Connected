using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Connectivity
{
	public abstract class ClientConnection : TenantObject, IDisposable
	{
		public event EventHandler Connected;
		protected ClientConnection()
		{
		}

		protected ClientConnection(ITenant tenant) : base(tenant)
		{
		}

		protected CancellationTokenSource Cancel { get; } = new CancellationTokenSource();
		protected HubConnection Hub { get; private set; }
		protected abstract string HubName { get; }
		protected abstract string Url { get; }
		protected virtual Action<HttpConnectionOptions> Options => null;
		public async void Connect()
		{
			if (Hub != null)
				Disconnect();

			Hub = Options == null
				 ? new HubConnectionBuilder().WithUrl(Url).Build()
				 : new HubConnectionBuilder().WithUrl(Url, Options).Build();

			Initialize();

			Hub.Closed += OnClosed;

			await ConnectAsync();
		}

		private Task OnClosed(Exception arg)
		{
			Cancel.Cancel();

			Connect();

			return Task.CompletedTask;
		}

		protected virtual void Initialize()
		{
		}

		public void Disconnect()
		{
			if (Hub == null)
				return;

			Task.FromResult(Hub.DisposeAsync()).GetAwaiter().GetResult();

			Hub = null;
		}

		private async Task<bool> ConnectAsync()
		{
			while (true)
			{
				if (Instance.State != InstanceState.Running)
				{
					await Task.Delay(50);
					continue;
				}

				try
				{
					if (Hub.State != HubConnectionState.Disconnected)
						return false;

					await Hub.StartAsync();

					Connected?.Invoke(this, EventArgs.Empty);
					OnConnected();

					return true;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
				catch
				{
					await Task.Delay(2000);
				}
			}
		}

		protected virtual void OnConnected()
		{

		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			GC.SuppressFinalize(this);

			if (disposing)
				Disconnect();
		}
		public bool IsConnected => Hub != null && Hub.State == HubConnectionState.Connected;
	}
}
