using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Services
{
	internal class TransactionBlock : ITransactionBlock
	{
		public Guid Transaction {get;set;}

		public Guid Partition {get;set;}

		public Guid Token {get;set;}
	}
}
