using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TomPIT.BigData.Data
{
	internal abstract class PartitionSchemaField
	{
		public string Name { get; set; }
		public bool Key { get; set; }
		public bool Index { get; set; }

		public Type Type { get; set; }
	}
}
