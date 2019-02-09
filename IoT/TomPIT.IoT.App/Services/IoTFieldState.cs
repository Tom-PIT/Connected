using System;

namespace TomPIT.IoT.Services
{
	internal class IoTFieldState : IoTFieldStateModifier, IIoTFieldState
	{
		public DateTime Modified { get; set; }
	}
}
