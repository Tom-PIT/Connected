using System;

namespace TomPIT.BigData
{
	public interface ITransactionBlock
	{
		Guid Transaction { get; }
		Guid Partition { get; }
		Guid Token { get; }
		Guid Timezone { get; }
	}
}
