using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.BigData
{
	public class PartitionFieldStatisticArgs : EventArgs
	{
		public PartitionFieldStatisticArgs(Guid file, string fieldName)
		{
			File = file;
			FieldName = fieldName;
		}

		public Guid File { get; }
		public string FieldName { get; }
	}
}
