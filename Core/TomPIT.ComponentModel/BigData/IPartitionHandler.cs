using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.BigData;
using TomPIT.Services;

namespace TomPIT.BigData
{
	public enum TimestampBehavior
	{
		Static = 1,
		Dynamic = 2
	}

	public interface IPartitionHandler<T> : IProcessHandler
	{
		TimestampBehavior Timestamp { get; }

		List<T> Invoke(List<T> items);
	}
}
