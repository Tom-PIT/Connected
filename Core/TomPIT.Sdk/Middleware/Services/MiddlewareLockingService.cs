using System;
using TomPIT.Data;

namespace TomPIT.Middleware.Services
{
	internal class MiddlewareLockingService : MiddlewareObject, IMiddlewareLockingService
	{
		public MiddlewareLockingService(IMiddlewareContext context) : base(context)
		{
		}

		public ILock Lock(string entity, TimeSpan timeout)
		{
			return Context.Tenant.GetService<ILockingService>().Lock(entity, timeout, 10);
		}

		public ILock Lock(string entity)
		{
			return Lock(entity, TimeSpan.FromSeconds(15));
		}

		public void Ping(Guid unlockKey)
		{
			Ping(unlockKey, TimeSpan.FromSeconds(10));
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			Context.Tenant.GetService<ILockingService>().Ping(unlockKey, timeout);
		}

		public void Ping(ILock unlockKey)
		{
			if (unlockKey == null)
				return;

			Ping(unlockKey.UnlockKey);
		}

		public void Ping(ILock unlockKey, TimeSpan timeout)
		{
			if (unlockKey == null)
				return;

			Ping(unlockKey.UnlockKey, timeout);
		}

		public void Unlock(ILock lockEntity)
		{
			if (lockEntity == null)
				return;

			Unlock(lockEntity.UnlockKey);
		}

		public void Unlock(Guid unlockKey)
		{
			Context.Tenant.GetService<ILockingService>().Unlock(unlockKey);
		}
	}
}
