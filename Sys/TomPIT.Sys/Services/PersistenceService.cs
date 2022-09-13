using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
	internal abstract class PersistenceService : HostedService
	{
		protected PersistenceService()
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

		protected override async Task OnExecute(CancellationToken cancel)
		{
			try
			{
				await OnPersist(cancel);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		protected virtual async Task OnPersist(CancellationToken cancel)
		{
			await Task.CompletedTask;
		}
	}
}
