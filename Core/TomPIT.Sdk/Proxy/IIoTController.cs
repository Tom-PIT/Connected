using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using TomPIT.IoT;

namespace TomPIT.Proxy
{
	public interface IIoTController
	{
		ImmutableList<IIoTFieldState> SelectState(Guid hub);
		void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields);
	}
}
