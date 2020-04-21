using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Analytics;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;

namespace TomPIT.Distributed
{
	internal class FlushingService : HostedService
	{
		public FlushingService()
		{
			IntervalTimeout = TimeSpan.FromSeconds(3);
		}

		protected override Task Process(CancellationToken token)
		{
			try
			{
				var connections = Shell.GetService<IConnectivityService>().QueryTenants();

				Parallel.ForEach(connections,
					(i) =>
					{
						if (token.IsCancellationRequested)
							return;

						var connection = Shell.GetService<IConnectivityService>().SelectTenant(i.Url);

						if (connection.GetService<ILoggingService>() is LoggingService l)
							l.Flush();

						if (connection.GetService<IMetricService>() is MetricService m)
							m.Flush();

						if (connection.GetService<IAnalyticsService>() is AnalyticsService a)
							((MruService)a.Mru).Flush();
					});
			}
			catch
			{
				//TODO: log exception
			}

			return Task.CompletedTask;

		}
	}
}
