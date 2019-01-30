using System;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;

namespace TomPIT.Services
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
				var connections = Shell.GetService<IConnectivityService>().QueryConnections();

				Parallel.ForEach(connections,
					(i) =>
					{
						var connection = Shell.GetService<IConnectivityService>().Select(i.Url);

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
