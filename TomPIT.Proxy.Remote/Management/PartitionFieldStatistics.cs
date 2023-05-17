using System;
using TomPIT.BigData;

namespace TomPIT.Proxy.Remote.Management
{
	internal class PartitionFieldStatistics : IPartitionFieldStatistics
	{
		public Guid File { get; set; }
		public string Key { get; set; }

		public string StartString { get; set; }

		public string EndString { get; set; }

		public decimal StartNumber { get; set; }

		public decimal EndNumber { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public string FieldName { get; set; }
		public Guid Partition { get; set; }
	}
}