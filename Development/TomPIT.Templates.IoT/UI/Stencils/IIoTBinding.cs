using TomPIT.ComponentModel;

namespace TomPIT.IoT.UI.Stencils
{
	public interface IIoTBinding : IConfigurationElement
	{
		string Field { get; }
	}
}
