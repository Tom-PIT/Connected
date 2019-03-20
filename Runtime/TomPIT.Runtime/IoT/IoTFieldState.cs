using System;

namespace TomPIT.IoT
{
	public class IoTFieldState : IoTFieldStateModifier, IIoTFieldState
	{
		public DateTime Modified { get; set; }
	}
}
