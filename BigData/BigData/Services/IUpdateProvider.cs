using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.BigData.Data;

namespace TomPIT.BigData.Services
{
	internal interface IUpdateProvider
	{
		ITransactionBlock Block { get; }
		PartitionSchema Schema { get; }
	}
}
