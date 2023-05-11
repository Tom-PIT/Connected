using System;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Data
{
	internal class LockingService : TenantObject, ILockingService
	{
		public LockingService(ITenant tenant) : base(tenant)
		{
		}

		public ILock Lock(string entity, TimeSpan timeout, int retryCount)
		{
			for (var i = 0; i < retryCount; i++)
			{
				var result = Instance.SysProxy.Locking.Lock(entity, timeout);

				if (result is not null)
					return result;

				Task.Delay(250).Wait();
			}

			throw new RuntimeException($"{SR.ErrLockFail} ({entity})");
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			Instance.SysProxy.Locking.Ping(unlockKey, timeout);
		}

		public void Unlock(Guid unlockKey)
		{
			Instance.SysProxy.Locking.Unlock(unlockKey);
		}
	}
}
