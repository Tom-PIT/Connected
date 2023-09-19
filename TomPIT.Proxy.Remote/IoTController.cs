using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.IoT;

namespace TomPIT.Proxy.Remote
{
	internal class IoTController : IIoTController
	{
		private const string Controller = "IoT";
		public ImmutableList<IIoTFieldState> SelectState(Guid hub)
		{
			return Connection.Post<List<IoTFieldState>>(Connection.CreateUrl(Controller, "SelectState"), new
			{
				hub
			}).ToImmutableList<IIoTFieldState>();
		}

		public void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			Connection.Post(Connection.CreateUrl(Controller, "UpdateState"), new
			{
				hub,
				fields
			});
		}
	}
}
