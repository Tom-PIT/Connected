using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.BigData
{
	public class PartitionFileArgs : EventArgs
	{
		public PartitionFileArgs(Guid fileName)
		{
			FileName = fileName;
		}

		public Guid FileName { get; }
	}
}
