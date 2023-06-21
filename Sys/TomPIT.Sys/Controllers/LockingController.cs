using System;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Data;
using TomPIT.Sys.Model;

using DataModel = TomPIT.Sys.Model.DataModel;

namespace TomPIT.Sys.Controllers
{
	public class LockingController : SysController
	{
		[HttpPost]
		public ILock Lock()
		{
			var body = FromBody();
			var entity = body.Required<string>("entity");
			var timeout = body.Required<TimeSpan>("timeout");

			return DataModel.Locking.Lock(entity, timeout);
		}

		[HttpPost]
		public void Unlock()
		{
			var body = FromBody();
			var key = body.Required<Guid>("unlockKey");

			DataModel.Locking.Unlock(key);
		}

		[HttpPost]
		public void Ping()
		{
			var body = FromBody();
			var key = body.Required<Guid>("unlockKey");
			var timeout = body.Required<TimeSpan>("timeout");

			DataModel.Locking.Ping(key, timeout);
		}
	}
}
