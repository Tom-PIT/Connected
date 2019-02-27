using System;

namespace TomPIT.Development
{
	public enum LockInfoResult
	{
		NoAction = 1,
		ShouldLock = 2,
		Locked = 3
	}

	public interface ILockInfo
	{
		LockInfoResult Result { get; }
		Guid Owner { get; }
	}
}
