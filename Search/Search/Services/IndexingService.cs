using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Distributed;
using TomPIT.Environment;

namespace TomPIT.Search.Services
{
	internal class IndexingService : HostedService
	{
		public IndexingService()
		{
			IntervalTimeout = TimeSpan.FromMilliseconds(490);
		}

		private IndexingDispatcher Dispatcher { get; set; }

		protected override bool OnInitialize(CancellationToken cancel)
		{
			if (Instance.State == InstanceState.Initializing)
				return false;

			Dispatcher = new IndexingDispatcher(Tenant.GetService<IResourceGroupService>().Default.Name);

			return true;
		}

		protected override async Task OnExecute(CancellationToken cancel)
		{
			var jobs = Instance.SysProxy.Management.Search.Dequeue(Dispatcher.Available);

			if (jobs is null)
				return;

			foreach (var i in jobs)
			{
				if (cancel.IsCancellationRequested)
					return;

				Dispatcher.Enqueue(i);
			}

			await Task.CompletedTask;
		}

		public override void Dispose()
		{
			Dispatcher?.Dispose();

			base.Dispose();
		}
	}
}
