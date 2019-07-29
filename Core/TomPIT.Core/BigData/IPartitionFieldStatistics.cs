using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.BigData
{
	public interface IPartitionFieldStatistics
	{
		Guid File { get; }
		string StartString { get; }
		string EndString { get; }
		double StartNumber { get; }
		double EndNumber { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		string FieldName { get; }
	}
}
