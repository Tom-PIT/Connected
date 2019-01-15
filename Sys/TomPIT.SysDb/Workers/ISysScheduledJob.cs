using System;
using TomPIT.Services;

namespace TomPIT.SysDb.Workers
{
	public interface ISysScheduledJob : IScheduledJob
	{
		Guid State { get; }
	}
}
