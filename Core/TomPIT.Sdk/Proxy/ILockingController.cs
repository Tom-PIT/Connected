using System;
using TomPIT.Data;

namespace TomPIT.Proxy
{
	public interface ILockingController
	{
		ILock Lock(string entity, TimeSpan timeout);
		void Unlock(Guid unlockKey);
		void Ping(Guid unlockKey, TimeSpan timeout);
	}
}
