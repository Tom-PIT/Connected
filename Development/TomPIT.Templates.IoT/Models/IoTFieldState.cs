using System;

namespace TomPIT.IoT.Models
{
	internal class IoTFieldState : IIoTFieldState
	{
		public DateTime Modified { get; set; }
		public string Field { get; set; }
		public string Value { get; set; }
	}
}
