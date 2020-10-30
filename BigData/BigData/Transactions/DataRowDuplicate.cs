using System;
using System.Data;

namespace TomPIT.BigData.Transactions
{
	internal class DataRowDuplicate
	{
		public DataRow Row { get; set; }
		public int Index { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
