using TomPIT.IoT;

namespace TomPIT.Proxy.Remote
{
	internal class IoTFieldStateModifier : IIoTFieldStateModifier
	{
		public string Field { get; set; }
		public string Value { get; set; }
		public string Device { get; set; }
		public object RawValue { get; set; }
	}
}
