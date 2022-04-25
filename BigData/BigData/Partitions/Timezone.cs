using System;

namespace TomPIT.BigData.Partitions
{
	internal class Timezone : ITimezone
	{
		public string Name { get; set; }

		public int Offset { get; set; }

		public Guid Token { get; set; }
	}
}
