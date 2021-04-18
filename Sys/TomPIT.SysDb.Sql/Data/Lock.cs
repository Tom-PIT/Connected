using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Sql.Data
{
	internal class Lock : TomPIT.Data.Sql.LongPrimaryKeyRecord, ILock
	{
		public string Entity { get; set; }

		public Guid UnlockKey { get; set; }

		public DateTime Timeout { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Entity = GetString("entity");
			UnlockKey = GetGuid("unlock_key");
			Timeout = GetDate("lock_timeout");
		}
	}
}
