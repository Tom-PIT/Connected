using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Services
{
	internal class Partition : IPartition
	{
		public Guid Configuration {get;set;}

		public int FileCount {get;set;}

		public PartitionStatus Status {get;set;}

		public string Name {get;set;}

		public DateTime Created {get;set;}

		public Guid ResourceGroup {get;set;}
	}
}
