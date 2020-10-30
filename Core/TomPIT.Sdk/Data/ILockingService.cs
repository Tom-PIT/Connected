using System;

namespace TomPIT.Data
{
	public interface ILockingService
	{
		ILock Lock(string entity, TimeSpan timeout, int retryCount);
		void Unlock(Guid unlockKey);
		void Ping(Guid unlockKey, TimeSpan timeout);
	}
}
