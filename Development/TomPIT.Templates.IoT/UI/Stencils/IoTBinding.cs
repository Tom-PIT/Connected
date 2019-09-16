using TomPIT.ComponentModel;

namespace TomPIT.MicroServices.IoT.UI.Stencils
{
	public class IoTBinding : ConfigurationElement, IIoTBinding
	{
		public string Field { get; set; }
	}
}
