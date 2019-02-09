using System;
using System.Collections.Generic;
using TomPIT.IoT;

namespace TomPIT.SysDb.IoT
{
	public interface IIoTHandler
	{
		List<IIoTFieldState> SelectState(Guid hub);
		void UpdateState(Guid hub, List<IIoTFieldStateModifier> fields);
	}
}
