using System;

namespace TomPIT.Data
{
	public interface IPopReceiptRecord
	{
		Guid PopReceipt { get; }
		DateTime NextVisible { get; }
	}
}
