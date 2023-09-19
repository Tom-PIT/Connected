using System;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Configuration;
using TomPIT.Diagnostics;
using TomPIT.Distributed;
using TomPIT.Middleware;

namespace TomPIT.Cdn.Mail
{
	internal class SmtpService : HostedService
	{
		private Socket _listener;
		private ManualResetEvent _allDone = new ManualResetEvent(false);
		private CancellationTokenSource _cancel = new CancellationTokenSource();

		public SmtpService()
		{
			IntervalTimeout = TimeSpan.Zero;
		}

		private CancellationTokenSource Cancel => _cancel;
		protected override Task OnExecute(CancellationToken cancel)
		{
			return Task.CompletedTask;
		}

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			Task.Run(() =>
			{
				Run();
			});

			return true;
		}

		public static string HostName { get; private set; }
		public static string Greeting { get; private set; }
		public static string Endpoint { get; private set; }
		internal static ImmutableArray<byte> DkimPrivateKey { get; private set; }
		internal static string DkimSelector { get; private set; }
		internal static string DkimDomain { get; private set; }
		internal static string DefaultEmailSender { get; private set; }
		private void Run()
		{
			var tenant = MiddlewareDescriptor.Current.Tenant;
			var token = Cancel.Token;

			var dkim = tenant.GetService<ISettingService>().GetValue<string>("DkimPrivateKey", null, null, null);

			if (!string.IsNullOrWhiteSpace(dkim))
				DkimPrivateKey = Encoding.UTF8.GetBytes(dkim).ToImmutableArray();

			DkimSelector = tenant.GetService<ISettingService>().GetValue<string>("DkimSelector", null, null, null);//_tompitsmtp
			DkimDomain = tenant.GetService<ISettingService>().GetValue<string>("DkimDomain", null, null, null);
			DefaultEmailSender = tenant.GetService<ISettingService>().GetValue<string>("DefaultEmailSender", null, null, null);

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
					Cancel.Cancel();

					if (_listener != null)
					{
						_listener.Disconnect(true);
						_listener = null;
					}

					Cancel.Dispose();
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
