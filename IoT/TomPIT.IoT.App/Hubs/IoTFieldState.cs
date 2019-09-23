using System;

namespace TomPIT.IoT.Hubs
{
	internal class IoTFieldState : IoTFieldStateModifier, IIoTFieldState
	{
		public DateTime Modified { get; set; }
	}
}
