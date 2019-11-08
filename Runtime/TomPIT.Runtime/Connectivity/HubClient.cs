using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace TomPIT.Connectivity
{
	public abstract class HubClient : TenantObject
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();

		public HubClient(ITenant tenant, string authenticationToken) : base(tenant)
		{
			AuthenticationToken = authenticationToken;
		}
		protected HubConnection Hub { get; private set; }
		protected string AuthenticationToken { get; }
		protected abstract string HubName { get; }

		public async void Connect()
		{
			if (Hub != null)
				Disconnect();

			_cancel = new CancellationTokenSource();
			Hub = new HubConnectionBuilder().WithUrl(string.Format("{0}/{1}", Tenant.Url, HubName), f =>
			{
				f.AccessTokenProvider = () =>
				{
					return Task.FromResult(AuthenticationToken);
				};

				f.Headers.Add("TomPITInstanceId", Instance.Id.ToString());

			}).Build();

			//.ConfigureLogging((o) =>
			// {
			//	 o.AddDebug();
			//	 o.SetMinimumLevel(LogLevel.Debug);
			// })
			Initialize();

			Hub.Closed += OnClosed;

			await ConnectAsync();
		}

		private Task OnClosed(Exception arg)
		{
			_cancel.Cancel();

			Connect();

			return Task.CompletedTask;
		}

		protected virtual void Initialize()
		{
		}

		private void Heartbeat()
		{
			while (!_cancel.IsCancellationRequested)
			{
				try
				{
					if (Hub == null)
						return;

					Hub.InvokeAsync("Heartbeat").Wait();
				}
				finally
				{
					_cancel.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30));
				}
			}
		}

		public void Disconnect()
		{
			if (Hub == null)
				return;

			Task.FromResult(Hub.DisposeAsync()).GetAwaiter().GetResult();
		}

		private async Task<bool> ConnectAsync()
		{
			while (true)
			{
				try
				{
					await Hub.StartAsync();

					new Task(Heartbeat, _cancel.Token, TaskCreationOptions.LongRunning).Start();

					return true;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
				catch
				{
					await Task.Delay(250);
				}
			}
		}
	}
}
