using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;

namespace TomPIT.Cdn.Services
{
	internal class ConnectionCleanupService : HostedService
	{
		private CancellationTokenSource _cancel = new CancellationTokenSource();
		private Lazy<List<MailDispatcher>> _dispatchers = new Lazy<List<MailDispatcher>>();

		public ConnectionCleanupService()
		{
			IntervalTimeout = TimeSpan.FromMinutes(1);
		}

		protected override Task Process()
		{
			ConnectionPool.CleanUp();

			return Task.CompletedTask;
		}
	}
}
