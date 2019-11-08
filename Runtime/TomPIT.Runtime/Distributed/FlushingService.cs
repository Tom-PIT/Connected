using System;
using System.Threading.Tasks;
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

		protected override Task Process()
		{
			try
			{
				var connections = Shell.GetService<IConnectivityService>().QueryTenants();

				Parallel.ForEach(connections,
					(i) =>
					{
						var connection = Shell.GetService<IConnectivityService>().SelectTenant(i.Url);

						if (connection.GetService<ILoggingService>() is LoggingService l)
							l.Flush();

						if (connection.GetService<IMetricService>() is MetricService m)
							m.Flush();
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
