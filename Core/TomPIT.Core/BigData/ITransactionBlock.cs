using System;

namespace TomPIT.BigData
{
	public interface ITransactionBlock
	{
		Guid Transaction { get; }
		int WorkerRemaining { get; }
		Guid Token { get; }
	}
}
