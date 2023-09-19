using System;
using TomPIT.Data;

namespace TomPIT.Proxy.Remote
{
	internal class LockingController : ILockingController
	{
		private const string Controller = "Locking";
		public ILock Lock(string entity, TimeSpan timeout)
		{
			return Connection.Post<Lock>(Connection.CreateUrl(Controller, "Lock"), new
			{
				entity,
				timeout
			});
		}

		public void Ping(Guid unlockKey, TimeSpan timeout)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Ping"), new
			{
				unlockKey,
				timeout
			});
		}

		public void Unlock(Guid unlockKey)
		{
			Connection.Post(Connection.CreateUrl(Controller, "Unlock"), new
			{
				unlockKey
			});
		}
	}
}
