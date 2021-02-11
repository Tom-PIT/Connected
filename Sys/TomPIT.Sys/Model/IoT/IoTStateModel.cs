using System;
using System.Collections.Generic;
using TomPIT.Caching;
using TomPIT.IoT;
using TomPIT.Sys.Api.Database;
using TomPIT.Sys.Notifications;

namespace TomPIT.Sys.Model.IoT
{
	public class IoTStateModel : CacheRepository<List<IIoTFieldState>, Guid>
	{
		public IoTStateModel(IMemoryCache container) : base(container, "iotstate")
		{

		}

		public List<IIoTFieldState> Select(Guid hub)
		{
			return Get(hub,
				(f) =>
				{
					return Shell.GetService<IDatabaseService>().Proxy.IoT.SelectState(hub);
				});
		}

		public void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			Shell.GetService<IDatabaseService>().Proxy.IoT.UpdateState(hub, fields);
			Remove(hub);
			IoTNotifications.IoTStateChanged(hub, fields);
		}
	}
}
