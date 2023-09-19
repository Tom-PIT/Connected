using System;
using TomPIT.Data;
using DataModel = TomPIT.Sys.Model.DataModel;

namespace TomPIT.Proxy.Local
{
	internal class LockingController : ILockingController
	{
		public ILock Lock(string entity, TimeSpan timeout)
		{
			return DataModel.Locking.Lock(entity, timeout);
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			DataModel.Locking.Ping(unlockKey, timeout);
		}

		public void Unlock(Guid unlockKey)
		{
			DataModel.Locking.Unlock(unlockKey);
		}
	}
}
