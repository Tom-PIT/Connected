using TomPIT.IoT;

namespace TomPIT.Sys.Model.IoT
{
	internal class IoTFieldState : IIoTFieldStateModifier
	{
		public string Field { get; set; }
		public string Value { get; set; }
		public string Device { get; set; }

		public object RawValue { get; set; }
	}
}
