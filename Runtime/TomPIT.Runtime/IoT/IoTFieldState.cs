using System;

namespace TomPIT.IoT
{
	internal class IoTFieldState : IIoTFieldState
	{
		public DateTime Modified { get; set; }
		public string Field { get; set; }
		public string Value { get; set; }
		public string Device { get; set; }
		public object RawValue { get; set; }
	}
}
