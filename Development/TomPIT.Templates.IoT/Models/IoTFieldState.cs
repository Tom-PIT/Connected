using System;
using TomPIT.IoT;

namespace TomPIT.MicroServices.IoT.Models
{
	internal class IoTFieldState : IIoTFieldState
	{
		public DateTime Modified { get; set; }
		public string Field { get; set; }
		public string Value { get; set; }
		public string Device { get; set; }
	}
}
