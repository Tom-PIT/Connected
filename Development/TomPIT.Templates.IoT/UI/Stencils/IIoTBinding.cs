using TomPIT.ComponentModel;

namespace TomPIT.MicroServices.IoT.UI.Stencils
{
	public interface IIoTBinding : IConfigurationElement
	{
		string Field { get; }
	}
}
