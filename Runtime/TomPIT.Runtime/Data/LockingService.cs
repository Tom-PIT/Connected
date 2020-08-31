using System;
using System.Threading.Tasks;
using TomPIT.Connectivity;
using TomPIT.Exceptions;
using TomPIT.Middleware;

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
				var result = Tenant.Post<Lock>(CreateUrl("Lock"), new
				{
					Entity = entity,
					Timeout = timeout
				});

				if (result != null)
					return result;

				Task.Delay(250).Wait();
			}

			throw new RuntimeException($"{SR.ErrLockFail} ({entity})");
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			Tenant.Post<Lock>(CreateUrl("Ping"), new
			{
				UnlockKey = unlockKey,
				Timeout = timeout
			});
		}

		public void Unlock(Guid unlockKey)
		{
			Tenant.Post(CreateUrl("Unlock"), new
			{
				UnlockKey = unlockKey
			});
		}

		private string CreateUrl(string action)
		{
			return Tenant.CreateUrl("Locking", action);
		}
	}
}
