using System;
using TomPIT.BigData;

namespace TomPIT.Proxy.Remote.Management
{
	internal class TransactionBlock : ITransactionBlock
	{
		public Guid Transaction { get; set; }

		public Guid Partition { get; set; }

		public Guid Token { get; set; }
		public Guid Timezone { get; set; }
	}
}
