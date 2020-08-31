using System;
using TomPIT.Data;

namespace TomPIT.Middleware.Services
{
	public interface IMiddlewareLockingService
	{
		ILock Lock(string entity);
		ILock Lock(string entity, TimeSpan timeout);
		void Unlock(ILock lockEntity);
		void Unlock(Guid unlockKey);
		void Ping(Guid unlockKey);
		void Ping(Guid unlockKey, TimeSpan timeout);
		void Ping(ILock unlockKey);
		void Ping(ILock unlockKey, TimeSpan timeout);
	}
}
