using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;

namespace TomPIT.Cdn.Mail
{
	internal class SmtpConnectionCleanupService : HostedService
	{
		private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

		public SmtpConnectionCleanupService()
		{
			IntervalTimeout = TimeSpan.FromMinutes(1);
		}

		protected override Task Process(CancellationToken cancel)
		{
			SmtpConnectionPool.CleanUp();

			return Task.CompletedTask;
		}
	}
}
