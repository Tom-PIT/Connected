using TomPIT.IoT;

namespace TomPIT.Sys.Data
{
	internal class IoTFieldState : IIoTFieldStateModifier
	{
		public string Field { get; set; }
		public string Value { get; set; }
	}
}
