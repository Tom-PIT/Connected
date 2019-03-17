using System;

namespace TomPIT.BigData
{
	public class PartitionArgs : EventArgs
	{
		public PartitionArgs(Guid configuration)
		{
			Configuration = configuration;
		}

		public Guid Configuration { get; }
	}
}
