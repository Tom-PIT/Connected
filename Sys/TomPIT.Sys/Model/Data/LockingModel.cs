using System;
using TomPIT.Data;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Model.Data
{
	internal class LockingModel
	{
		public ILock Lock(string entity, TimeSpan timeout)
		{
			return Shell.GetService<IDatabaseService>().Proxy.Data.Locking.Lock(entity, DateTime.UtcNow.Add(timeout), DateTime.UtcNow);
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			Shell.GetService<IDatabaseService>().Proxy.Data.Locking.Ping(unlockKey, DateTime.UtcNow.Add(timeout));
		}

		public void Unlock(Guid unlockKey)
		{
			Shell.GetService<IDatabaseService>().Proxy.Data.Locking.Unlock(unlockKey);
		}
	}
}
