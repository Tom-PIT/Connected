﻿using TomPIT.IoT;

namespace TomPIT.Sys.Data.IoT
{
	internal class IoTFieldState : IIoTFieldStateModifier
	{
		public string Field { get; set; }
		public string Value { get; set; }
	}
}