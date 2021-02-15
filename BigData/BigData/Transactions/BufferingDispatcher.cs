using System.Threading;
using TomPIT.Distributed;

namespace TomPIT.BigData.Transactions
{
	internal class BufferingDispatcher : Dispatcher<IPartitionBuffer>
	{
		public BufferingDispatcher(string resourceGroup) : base(32)
		{
			ResourceGroup = resourceGroup;
		}

		public override DispatcherJob<IPartitionBuffer> CreateWorker(IDispatcher<IPartitionBuffer> owner, CancellationToken cancel)
		{
			return new BufferingJob(owner, cancel);
		}

		public string ResourceGroup { get; }
	}
}
