using System.Threading;
using TomPIT.Distributed;

namespace TomPIT.BigData.Transactions
{
	internal class BufferingDispatcher : Dispatcher<IPartitionBuffer>
	{
		public BufferingDispatcher(string resourceGroup, CancellationToken cancel) : base(cancel, 32)
		{
			ResourceGroup = resourceGroup;
		}

		protected override DispatcherJob<IPartitionBuffer> CreateWorker(CancellationToken cancel)
		{
			return new BufferingJob(this, cancel);
		}

		public string ResourceGroup { get; }
	}
}
