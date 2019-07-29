using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchemaNumberField:PartitionSchemaField
	{
		public AggregateMode Aggregate { get; set; } = AggregateMode.None;
	}
}
