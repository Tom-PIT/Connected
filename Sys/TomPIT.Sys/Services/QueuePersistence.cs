using System;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
	internal class QueuePersistence : PersistenceService
	{
		public QueuePersistence()
		{
			IntervalTimeout = TimeSpan.Zero;
		}

		protected override async Task OnPersist(CancellationToken cancel)
		{
			await DataModel.Queue.Flush();
		}
	}
}
