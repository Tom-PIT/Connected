using System;
using TomPIT.IoT;

namespace TomPIT.Connectivity
{
	internal class IoTFieldState : IoTFieldStateModifier, IIoTFieldState
	{
		public DateTime Modified { get; set; }
	}
}
