using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.IoT;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local
{
	internal class IoTController : IIoTController
	{
		public ImmutableList<IIoTFieldState> SelectState(Guid hub)
		{
			return DataModel.IoTState.Select(hub).ToImmutableList();
		}

		public void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			DataModel.IoTState.UpdateState(hub, fields);
		}
	}
}
