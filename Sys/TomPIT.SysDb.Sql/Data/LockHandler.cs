using System;
using TomPIT.Data;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class LockHandler : ILockHandler
	{
		public ILock Lock(string entity, DateTime timeout, DateTime date)
		{
			var r = new Reader<Lock>("tompit.lock_lock");

			r.CreateParameter("@entity", entity);
			r.CreateParameter("@timeout", timeout);
			r.CreateParameter("@date", date);

			return r.ExecuteSingleRow();
		}

		public void Ping(Guid unlockKey, DateTime timeout)
		{
			var w = new Writer("tompit.lock_ping");

			w.CreateParameter("@unlock_key", unlockKey);
			w.CreateParameter("@timeout", timeout);

			w.Execute();
		}

		public void Unlock(Guid unlockKey)
		{
			var w = new Writer("tompit.lock_unlock");

			w.CreateParameter("@unlock_key", unlockKey);

			w.Execute();
		}
	}
}
