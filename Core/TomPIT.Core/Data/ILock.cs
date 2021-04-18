using System;

namespace TomPIT.Data
{
	public interface ILock
	{
		string Entity { get; }
		Guid UnlockKey { get; }
		DateTime Timeout { get; }
	}
}
