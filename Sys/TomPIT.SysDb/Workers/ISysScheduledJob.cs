using System;
using TomPIT.Distributed;

namespace TomPIT.SysDb.Workers
{
	public interface ISysScheduledJob : IScheduledJob
	{
		Guid State { get; }
	}
}
