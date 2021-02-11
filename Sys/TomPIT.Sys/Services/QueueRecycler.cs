using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
	internal class QueueRecycler : HostedService
	{
		public QueueRecycler()
		{
			IntervalTimeout = TimeSpan.FromHours(12);
		}
		protected override bool Initialize(CancellationToken cancel)
		{
			return DataModel.Initialized;
		}

		protected override Task Process(CancellationToken cancel)
		{
			try
			{
				DataModel.Queue.Recycle();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return Task.CompletedTask;
		}
	}
}
