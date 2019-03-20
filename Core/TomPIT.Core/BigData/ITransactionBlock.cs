using System;

namespace TomPIT.BigData
{
	public interface ITransactionBlock
	{
		Guid Transaction { get; }
		Guid Partition { get; }
		Guid Token { get; }
		DateTime NextVisible { get; }
		Guid PopReceipt { get; }
	}
}
