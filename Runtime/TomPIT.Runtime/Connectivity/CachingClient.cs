using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Connectivity
{
	internal class CachingClient
	{
		private HubConnection _connection = null;
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private CachingEvents _events = null;

		public CachingClient(ISysConnection connection, string authenticationToken)
		{
			Connection = connection;
			AuthenticationToken = authenticationToken;
		}

		private ISysConnection Connection { get; }
		private string AuthenticationToken { get; }

		public async void Connect()
		{
			if (_connection != null)
				Disconnect();

			_cancel = new CancellationTokenSource();
			_connection = new HubConnectionBuilder().WithUrl(string.Format("{0}/caching", Connection.Url), f =>
			 {
				 f.AccessTokenProvider = () =>
				 {
					 return Task.FromResult(AuthenticationToken);
				 };
			 }).ConfigureLogging(l =>
			 {
				 l.SetMinimumLevel(LogLevel.Debug);
				 l.AddConsole();
			 }).Build();

			HookEvents();

			_connection.Closed += OnClosed;

			await ConnectAsync();
		}

		private Task OnClosed(Exception arg)
		{
			_cancel.Cancel();

			Connect();

			return Task.CompletedTask;
		}

		private void HookEvents()
		{
			_events = new CachingEvents(Connection, _connection);

			_events.Hook();
		}

		private void Heartbeat()
		{
			while (!_cancel.IsCancellationRequested)
			{
				try
				{
					if (_connection == null)
						return;

					_connection.InvokeAsync("Heartbeat");
				}
				finally
				{
					_cancel.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30));
				}
			}
		}

		public void Disconnect()
		{
			if (_connection == null)
				return;

			Task.FromResult(_connection.DisposeAsync()).GetAwaiter().GetResult();
		}

		private async Task<bool> ConnectAsync()
		{
			while (true)
			{
				try
				{
					await _connection.StartAsync();

					new Task(Heartbeat, _cancel.Token, TaskCreationOptions.LongRunning).Start();

					return true;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
				catch
				{
					await Task.Delay(1000);
				}
			}
		}
	}
}
