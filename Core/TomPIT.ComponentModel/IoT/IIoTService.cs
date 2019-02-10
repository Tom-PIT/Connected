using System;
using System.Collections.Generic;

namespace TomPIT.IoT
{
	public interface IIoTService
	{
		List<IIoTFieldState> SelectState(Guid hub);
	}
}
