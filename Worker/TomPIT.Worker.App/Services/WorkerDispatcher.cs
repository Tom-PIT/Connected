﻿using System.Threading;
using TomPIT.Distributed;
using TomPIT.Storage;

namespace TomPIT.Worker.Services
{
	internal class WorkerDispatcher : Dispatcher<IQueueMessage>
	{
		public WorkerDispatcher(string resourceGroup) : base(256)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IQueueMessage> CreateWorker(IDispatcher<IQueueMessage> owner, CancellationToken cancel)
		{
			return new WorkerJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
