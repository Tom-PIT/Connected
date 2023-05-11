using Newtonsoft.Json;
using System;
using TomPIT.IoT;

namespace TomPIT.Proxy.Remote
{
	internal class IoTFieldState : IIoTFieldState
	{
		public DateTime Modified { get; set; }
		public string Field { get; set; }
		public string Value { get; set; }
		public string Device { get; set; }

		[JsonIgnore]
		public object RawValue { get; set; }
	}
}
