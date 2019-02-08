using TomPIT.ComponentModel;

namespace TomPIT.IoT.UI.Stencils
{
	public class IoTBinding : ConfigurationElement, IIoTBinding
	{
		public string Field { get; set; }
	}
}
