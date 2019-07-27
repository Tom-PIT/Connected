using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData
{
	public enum AggregateMode
	{
		None = 1,
		Sum = 2
	}

	public interface IBigDataService
	{
		void Add<T>(IPartitionConfiguration partition, List<T> items);
	}
}
