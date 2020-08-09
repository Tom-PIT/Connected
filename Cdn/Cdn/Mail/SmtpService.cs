using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Configuration;
using TomPIT.Diagostics;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Mail
{
	internal class SmtpService : HostedService
	{
		private Socket _listener;
		private ManualResetEvent _allDone = new ManualResetEvent(false);

		public SmtpService()
		{
			IntervalTimeout = TimeSpan.Zero;
		}
		protected override Task Process(CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initialining)
				return false;

			Task.Run(() =>
			{
				Run(cancel);
			});

			return true;
		}

		public static string HostName { get; private set; }
		public static string Greeting { get; private set; }
		public static string Endpoint { get; private set; }
		private void Run(CancellationToken token)
		{
			var tenant = MiddlewareDescriptor.Current.Tenant;
			Endpoint = tenant.GetService<ISettingService>().GetValue<string>("SmtpEndpoint", null, null, null);

			if (string.IsNullOrWhiteSpace(Endpoint))
			{
				tenant.LogWarning(SR.NoSmtpEndpoint);
				return;
			}

			HostName = tenant.GetService<ISettingService>().GetValue<string>("SmtpHostName", null, null, null);

			if (string.IsNullOrWhiteSpace(HostName))
			{
				tenant.LogWarning(SR.NoSmtpHostName);
				return;
			}

			Greeting = tenant.GetService<ISettingService>().GetValue<string>("SmtpGreeting", null, null, null);

			if (string.IsNullOrWhiteSpace(Greeting))
				Greeting = HostName;

			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				_listener.Bind(new IPEndPoint(IPAddress.Parse(Endpoint), 25));
				_listener.Listen(100);

				while (!token.IsCancellationRequested)
				{
					_allDone.Reset();

					if (_listener != null)
						_listener.BeginAccept(new AsyncCallback(AcceptCallback), _listener);

					_allDone.WaitOne();
				}
			}
			catch { }

		}

		public override Task StopAsync(CancellationToken cancel)
		{
			lock (this)
			{
				try
				{
					if (_listener != null)
					{
						_listener.Disconnect(true);
						_listener = null;
					}
				}
				catch
				{
				}
			}

			return base.StopAsync(cancel);
		}

		private void AcceptCallback(IAsyncResult ar)
		{
			_allDone.Set();

			var listener = (Socket)ar.AsyncState;
			var handler = listener.EndAccept(ar);

			using var transaction = new SmtpTransaction(handler);

			transaction.ProcessRequest();
		}
	}
}
