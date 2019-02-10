using TomPIT.IoT;

namespace TomPIT.Connectivity
{
	internal class IoTFieldStateModifier : IIoTFieldStateModifier
	{
		public string Field { get; set; }
		public string Value { get; set; }
	}
}
