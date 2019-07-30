using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchemaNumberField:PartitionSchemaField
	{
		public AggregateMode Aggregate { get; set; } = AggregateMode.None;

		public override int CompareTo(object obj)
		{
			if (!(obj is PartitionSchemaNumberField nf))
				return -1;

			if (Aggregate != nf.Aggregate)
				return -1;

			return base.CompareTo(obj);
		}
	}
}
