using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Sys.Data;
using TomPIT.SysDb.Workers;

namespace TomPIT.Sys.Workers
{
	internal class Scheduler : HostedService
	{
		public Scheduler()
		{
			IntervalTimeout = TimeSpan.FromSeconds(1);
		}
		protected override Task Process(CancellationToken cancel)
		{
			try
			{
				var ds = DataModel.Workers.QueryScheduled();

				foreach (var i in ds)
				{
					if (cancel.IsCancellationRequested)
						break;

					Enqueue(i);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				//TODO: log exception
			}

			return Task.CompletedTask;
		}

		private void Enqueue(ISysScheduledJob job)
		{
			DataModel.Workers.Enqueue(job);
		}
	}
}
