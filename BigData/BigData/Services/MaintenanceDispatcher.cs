using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.BigData.Services
{
	internal class MaintenanceDispatcher : Dispatcher<IQueueMessage>
	{
		public MaintenanceDispatcher(string resourceGroup, CancellationTokenSource cancel) : base(cancel, 4)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IQueueMessage> CreateWorker(CancellationTokenSource cancel)
		{
			return new MaintenanceJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
