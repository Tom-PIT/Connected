using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
	internal class QueueFlusher : HostedService
	{
		public QueueFlusher()
		{
			IntervalTimeout = TimeSpan.FromSeconds(15);
		}
		protected override bool OnInitialize(CancellationToken cancel)
		{
			return DataModel.Initialized;
		}

		public override async Task StopAsync(CancellationToken cancel)
		{
			await OnExecute(CancellationToken.None);
		}

		protected override Task OnExecute(CancellationToken cancel)
		{
			try
			{
				DataModel.Queue.Flush();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			return Task.CompletedTask;
		}
	}
}
