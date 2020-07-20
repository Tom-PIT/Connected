using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.Configuration;
using TomPIT.Distributed;
using TomPIT.Middleware;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Cdn.Mail
{
	internal class MailService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

		private void OnSettingChanged(object sender, SettingEventArgs e)
		{
			if (string.Compare(e.Name, "MailServiceTimer", true) == 0 && string.IsNullOrWhiteSpace(e.Type) && string.IsNullOrWhiteSpace(e.PrimaryKey))
				SetInterval();
		}

		protected override bool Initialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initialining)
				return false;

			SetInterval();

			MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().SettingChanged += OnSettingChanged;

			foreach (var i in Shell.GetConfiguration<IClientSys>().ResourceGroups)
				Dispatchers.Add(new MailDispatcher(i, cancel));

			return true;
		}
		private void SetInterval()
		{
			var interval = MiddlewareDescriptor.Current.Tenant.GetService<ISettingService>().GetValue<int>("MailServiceTimer", null, null);

			if (interval == 0)
				interval = 5000;

			IntervalTimeout = TimeSpan.FromMilliseconds(interval);
		}

		protected override Task Process(CancellationToken cancel)
		{
			Parallel.ForEach(Dispatchers, (f) =>
			{
				var url = MiddlewareDescriptor.Current.Tenant.CreateUrl("MailManagement", "Dequeue");

				var e = new JObject
				{
					{ "count", f.Available },
				};

				var messages = MiddlewareDescriptor.Current.Tenant.Post<List<MailMessage>>(url, e);

				if (cancel.IsCancellationRequested)
					return;

				if (messages == null)
					return;

				foreach (var i in messages)
				{
					if (cancel.IsCancellationRequested)
						return;

					f.Enqueue(i);
				}
			});

			return Task.CompletedTask;
		}

		private List<MailDispatcher> Dispatchers { get { return _dispatchers.Value; } }
	}
}
