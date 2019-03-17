using System;

namespace TomPIT.BigData
{
	public enum TransactionStatus
	{
		Pending = 1,
		Running = 2
	}

	public interface ITransaction
	{
		int BlockCount { get; }
		int BlobkRemaining { get; }
		DateTime Created { get; }
		Guid Token { get; }
		TransactionStatus Status { get; }
	}
}
