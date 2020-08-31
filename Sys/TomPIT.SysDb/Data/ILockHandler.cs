using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Data
{
	public interface ILockHandler
	{
		ILock Lock(string entity, DateTime timeout, DateTime date);
		void Ping(Guid unlockKey, DateTime timeout);
		void Unlock(Guid unlockKey);
	}
}
