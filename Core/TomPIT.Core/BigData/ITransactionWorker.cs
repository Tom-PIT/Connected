using System;

namespace TomPIT.BigData
{
	public enum TransactionWorkerStatus
	{
		Idle = 1,
		Activated = 2,
		Busy = 3
	}

	public interface ITransactionWorker
	{
		Guid Block { get; }
		TransactionWorkerStatus Status { get; }
		Guid Token { get; }
		Guid Configuration { get; }
		Guid NextVisible { get; }
		Guid PopReceipt { get; }
		bool HasDependencies { get; }
	}
}
