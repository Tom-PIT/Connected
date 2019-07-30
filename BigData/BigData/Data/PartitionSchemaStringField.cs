using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Data
{
	internal class PartitionSchemaStringField:PartitionSchemaField
	{
		public int Length { get; set; }

		public override int CompareTo(object obj)
		{
			if (!(obj is PartitionSchemaStringField sf))
				return -1;

			if (Length != sf.Length)
				return -1;

			return base.CompareTo(obj);
		}
	}
}
